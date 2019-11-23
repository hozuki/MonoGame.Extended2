using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using FFmpeg.AutoGen;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using VideoPlayer = MonoGame.Extended.Framework.Media.VideoPlayer;

namespace MonoGame.Extended.VideoPlayback {
    /// <summary>
    /// Provides some helper functions for FFmpeg decoding.
    /// </summary>
    internal static unsafe class FFmpegHelper {

        /// <summary>
        /// Required output format for <see cref="ffmpeg.sws_scale"/> (<see cref="AVPixelFormat.AV_PIX_FMT_RGB0"/>).
        /// This value correspond to the default texture surface format (<see cref="SurfaceFormat.Color"/>) in MonoGame.
        /// </summary>
        /// <seealso cref="VideoPlayer.RequiredSurfaceFormat"/>
        internal const AVPixelFormat RequiredPixelFormat = AVPixelFormat.AV_PIX_FMT_RGB0;

        /// <summary>
        /// Required output format for <see cref="ffmpeg.swr_convert"/> (<see cref="AVSampleFormat.AV_SAMPLE_FMT_S16"/>).
        /// This sample format fits <see cref="ALFormat.Stereo16"/>.
        /// </summary>
        internal const AVSampleFormat RequiredSampleFormat = AVSampleFormat.AV_SAMPLE_FMT_S16;

        /// <summary>
        /// Required number of channels of the output for <see cref="ffmpeg.swr_convert"/>.
        /// This channel count fits <see cref="ALFormat.Stereo16"/>.
        /// </summary>
        internal const int RequiredChannels = 2;

        /// <summary>
        /// Required sample rate for <see cref="ffmpeg.swr_convert"/>, in Hz.
        /// It can be other frequencies, such as 22050, 32000, 48000, 96000, etc.
        /// Considering that most of the devices support 44100 Hz output and the quality is reasonale, the sample rate is chosen as 44100 Hz.
        /// </summary>
        internal const int RequiredSampleRate = 44100;

        /// <summary>
        /// Required number of channels, using <see cref="AudioChannels"/> enum.
        /// </summary>
        /// <seealso cref="RequiredChannels"/>
        internal const AudioChannels RequiredChannelsXna = AudioChannels.Stereo;

        /// <summary>
        /// Verifies the error code returned by FFmpeg functions.
        /// If an error occurs, this method throws a <see cref="FFmpegException"/>.
        /// </summary>
        /// <param name="avError">The error code.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Verify(int avError) {
            Verify(avError, null);
        }

        /// <summary>
        /// Verifies the error code returned by FFmpeg functions.
        /// If an error occurs, this method calls a user-specified clean-up method, then throws a <see cref="FFmpegException"/>.
        /// </summary>
        /// <param name="avError">The error code.</param>
        /// <param name="cleanupProc">The clean-up method. Ignored if set to <see langword="null"/>.</param>
        /// <exception cref="FFmpegException">Thrown when <paramref name="avError"/> does not imply success.</exception>
        internal static void Verify(int avError, [CanBeNull] Action cleanupProc) {
            if (avError >= 0) {
                return;
            }

            cleanupProc?.Invoke();

            const ulong bufferSize = 1024;

            var errorBuffer = new byte[bufferSize];

            fixed (byte* eb = errorBuffer) {
                ffmpeg.av_strerror(avError, eb, bufferSize);
            }

            var tailIndex = Array.IndexOf(errorBuffer, (byte)0);

            if (tailIndex < 0) {
                tailIndex = errorBuffer.Length;
            }

            var errorString = Encoding.UTF8.GetString(errorBuffer, 0, tailIndex);

            throw new FFmpegException(errorString, avError);
        }

        /// <summary>
        /// Retrieves the duration information included in <see cref="AVFormatContext"/>, in seconds.
        /// </summary>
        /// <param name="context">The <see cref="AVFormatContext"/>, representing a media file.</param>
        /// <returns>Duration, in seconds.</returns>
        /// <remarks>
        /// Not all containers (what <see cref="AVFormatContext"/> represents) contain duration information.
        /// The best practice is use <see cref="AVStream.duration"/> to get the duration of a stream, because some media files has multiple streams with different lengths (e.g. director's cut and theater's cut in one .mkv).
        /// That method is accurate but requires user interaction to select the stream they want, also that situation is not so common. So here we still use <see cref="AVFormatContext"/> to get the duration.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static double GetDurationInSeconds([CanBeNull] AVFormatContext* context) {
            if (context == null) {
                return 0;
            }

            var duration = context->duration;

            return (double)duration / ffmpeg.AV_TIME_BASE;
        }

        /// <summary>
        /// Calculates the corresponding value of a PTS (presentation timestamp) in seconds, based on given <see cref="AVStream"/>'s time base.
        /// </summary>
        /// <param name="stream">The <see cref="AVStream"/> whose time base is going to be the calculation standard.</param>
        /// <param name="pts">Value of the presentation timestamp.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static double ConvertPtsToSeconds([CanBeNull] AVStream* stream, long pts) {
            if (stream == null) {
                return 0;
            }

            // The standard way is use AV_TIMEBASE_Q and av_rescale_q().
            // Well, that may be not necessary unless there is another breaking change of this in FFmpeg.
            var timeBase = stream->time_base;
            var result = (double)pts * timeBase.num / timeBase.den;

            return result;
        }

        /// <summary>
        /// Converts given time in seconds to stream timestamp, based on given <see cref="AVStream"/>'s time base.
        /// </summary>
        /// <param name="stream">The <see cref="AVStream"/> whose time base is going to be the calculation standard.</param>
        /// <param name="seconds">Time, in seconds.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static long ConvertSecondsToPts([CanBeNull] AVStream* stream, double seconds) {
            if (stream == null) {
                return 0;
            }

            var timeBase = stream->time_base;
            var result = (long)Math.Round(seconds * timeBase.den / timeBase.num);

            return result;
        }

        /// <summary>
        /// Converts given time in seconds to stream timestamp, based on FFmpeg's default time base (<see cref="ffmpeg.AV_TIME_BASE"/>).
        /// </summary>
        /// <param name="seconds">Time, in seconds.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static long ConvertSecondsToPts(double seconds) {
            return ConvertSecondsToPts(seconds, 0);
        }

        /// <summary>
        /// Converts given time in seconds to stream timestamp, based on FFmpeg's default time base (<see cref="ffmpeg.AV_TIME_BASE"/>), with an extra PTS offset.
        /// </summary>
        /// <param name="seconds">Time, in seconds.</param>
        /// <param name="startPts">Extra starting PTS. For example, the video starting PTS.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static long ConvertSecondsToPts(double seconds, long startPts) {
            var result = (long)Math.Round(seconds * ffmpeg.AV_TIME_BASE);

            result += startPts;

            return result;
        }

        /// <summary>
        /// Gets a null-ended string from its pointer.
        /// Similar to <see cref="Marshal.PtrToStringAnsi(IntPtr)"/>, but this function lets you choose a custom encoding.
        /// </summary>
        /// <param name="ptr">The pointer to the string.</param>
        /// <param name="encoding">The encoding of the string.</param>
        /// <returns>The string, in <see cref="string"/>.</returns>
        [CanBeNull]
        [ContractAnnotation("ptr:null=>null")]
        internal static string PtrToStringNullTerminated([CanBeNull] byte* ptr, [NotNull] Encoding encoding) {
            if (ptr == null) {
                return null;
            }

            var p = ptr;

            while (*p != 0) {
                ++p;
            }

            var length = (int)(p - ptr);
            var buf = new byte[length];

            Marshal.Copy(new IntPtr(ptr), buf, 0, length);

            return encoding.GetString(buf);
        }

        /// <summary>
        /// Transmit data from current video frame of a <see cref="DecodeContext"/> to a <see cref="Texture2D"/> using an auto allocated data buffer.
        /// The texture's surface format must be <see cref="SurfaceFormat.Color"/>.
        /// </summary>
        /// <param name="context">The <see cref="DecodeContext"/> </param>
        /// <param name="texture">The <see cref="Texture2D"/> to transmit to.</param>
        /// <returns><see langword="true"/> if all successful, otherwise <see langword="false"/>.</returns>
        /// <seealso cref="RequiredPixelFormat"/>
        internal static bool TransmitVideoFrame([NotNull] DecodeContext context, [NotNull] Texture2D texture) {
            var buffer = new uint[texture.Width * texture.Height];

            return TransmitVideoFrame(context, texture, buffer);
        }

        /// <summary>
        /// Transmit data from current video frame of a <see cref="DecodeContext"/> to a <see cref="Texture2D"/> using a custom data buffer.
        /// The buffer must have enough capacity to hold all the data of the texture.
        /// The texture's surface format must be <see cref="SurfaceFormat.Color"/>.
        /// </summary>
        /// <param name="context">The <see cref="DecodeContext"/> </param>
        /// <param name="texture">The <see cref="Texture2D"/> to transmit to.</param>
        /// <param name="buffer">The data buffer.</param>
        /// <returns><see langword="true"/> if all successful, otherwise <see langword="false"/>.</returns>
        /// <seealso cref="RequiredPixelFormat"/>
        internal static bool TransmitVideoFrame([NotNull] DecodeContext context, [NotNull] Texture2D texture, [NotNull] uint[] buffer) {
            if (texture.Format != VideoPlayer.RequiredSurfaceFormat) {
                return false;
            }

            var r = TransmitVideoFrame(context, texture.Width, texture.Height, buffer);

            if (!r) {
                return false;
            }

            texture.SetData(buffer);

            return true;
        }

        /// <summary>
        /// Transmit data from current video frame of a <see cref="DecodeContext"/> to a buffer.
        /// The buffer holds data for a <see cref="Texture2D"/>. It must have enough capacity to hold the data from a buffer with specified width and height, and format <see cref="SurfaceFormat.Color"/>.
        /// </summary>
        /// <param name="context">The <see cref="DecodeContext"/> </param>
        /// <param name="textureWidth">Width of the texture.</param>
        /// <param name="textureHeight">Height of the texture.</param>
        /// <param name="buffer">The data buffer.</param>
        /// <returns><see langword="true"/> if all successful, otherwise <see langword="false"/>.</returns>
        /// <seealso cref="RequiredPixelFormat"/>
        internal static bool TransmitVideoFrame([NotNull] DecodeContext context, int textureWidth, int textureHeight, [NotNull] uint[] buffer) {
            Trace.Assert(textureWidth > 0 && textureHeight > 0);

            var videoContext = context.VideoContext;

            if (videoContext == null) {
                return false;
            }

            // Fetch the latest decoded frame.
            // You should use a synchronizing mechanism to avoid race condition. See DecodeContext.VideoFrameTransmissionLock.
            var videoFrame = context.FetchAvailableVideoFrame();

            if (videoFrame == null) {
                return false;
            }

            // Here we use the function in FFmpeg to calculate it for us.
            // Note the pixel format parameter. We assume that the texture uses the same format.
            var requiredBufferSize = ffmpeg.av_image_get_buffer_size(RequiredPixelFormat, textureWidth, textureHeight, 0);

            if (buffer.Length * sizeof(uint) < requiredBufferSize) {
                return false;
            }

            // Get the SWS context.
            var scaleContext = videoContext.GetSuitableScaleContext(textureWidth, textureHeight);

            // The type conversion here provided by FFmpeg.AutoGen is invalid, so we have to write a little code...
            fixed (uint* pBuffer = buffer) {
                var dstData = new[] { (byte*)pBuffer };
                var dstStride = new[] { textureWidth * sizeof(uint) };

                // Perform scaling (if needed) and pixel format conversion (if needed), and copy the data to our buffer.
                // Thanks @Slow for pointing out the return value of sws_scale.
                // This contributes to the correct version of Verify() method. I should have read the docs more carefully.
                Verify(ffmpeg.sws_scale(scaleContext, videoFrame->data, videoFrame->linesize, 0, videoContext.CodecContext->height, dstData, dstStride));
            }

            return true;
        }

        /// <summary>
        /// Transmits the given audio frame to an auto allocated buffer. Returns whether the operation is success, and the buffer when it succeeds.
        /// </summary>
        /// <param name="context">The <see cref="DecodeContext"/> containing decoding information.</param>
        /// <param name="frame">The <see cref="AVFrame"/> containing decoded audio data.</param>
        /// <param name="buffer">An auto allocated buffer. If the function succeeds, it contains audio data of required format. The data is NOT planar.</param>
        /// <returns><see langword="true"/> if all operation succeeds, otherwise <see langword="false"/>.</returns>
        /// <seealso cref="RequiredChannels"/>
        /// <seealso cref="RequiredSampleFormat"/>
        /// <seealso cref="RequiredSampleRate"/>
        internal static bool TransmitAudioFrame([NotNull] DecodeContext context, [NotNull] AVFrame* frame, [CanBeNull] out byte[] buffer) {
            buffer = null;

            var audioContext = context.AudioContext;

            if (audioContext == null) {
                return false;
            }

            if (frame->nb_samples == 0 || frame->channels == 0) {
                return true;
            }

            const int dstChannels = RequiredChannels;
            const AVSampleFormat dstSampleFormat = RequiredSampleFormat;
            const int dstSampleRate = RequiredSampleRate;

            var resampleContext = audioContext.GetSuitableResampleContext(dstSampleFormat, dstChannels, dstSampleRate);

            // First roughly estimates the number of samples in the output data.
            var roughDstSampleCount = (int)ffmpeg.av_rescale_rnd(frame->nb_samples, dstSampleRate, audioContext.SampleRate, AVRounding.AV_ROUND_UP);

            if (roughDstSampleCount < 0) {
                throw new FFmpegException("Failed to calculate simple rescaled sample count.");
            }

            var dstSampleCount = roughDstSampleCount;

            // About dstData and being continuous:
            // We only care about 16-bit stereo audio, so the audio output always has 1 plane (not planar).
            // For more complicated situations: http://blog.csdn.net/dancing_night/article/details/45642107
            byte** dstData = null;
            var dstLineSize = 0;

            try {
                // Allocate channel array and sample buffers.
                Verify(ffmpeg.av_samples_alloc_array_and_samples(&dstData, &dstLineSize, dstChannels, dstSampleCount, dstSampleFormat, 0));

                Debug.Assert(dstData != null);

                // Then consider the possible resample delay and calculate the correct number of samples.
                // TODO: Isn't this redundant? We may use this value in the first place.
                dstSampleCount = (int)ffmpeg.av_rescale_rnd(ffmpeg.swr_get_delay(resampleContext, audioContext.SampleRate) + frame->nb_samples, dstSampleRate, audioContext.SampleRate, AVRounding.AV_ROUND_UP);

                if (dstSampleCount <= 0) {
                    throw new FFmpegException("Failed to calculate rescaled sample count (with possible delays).");
                }

                // If there is a delay, we have to adjust the buffers allocated. (Yeah actually one buffer.)
                if (dstSampleCount > roughDstSampleCount) {
                    ffmpeg.av_free(&dstData[0]);

                    Verify(ffmpeg.av_samples_alloc(dstData, &dstLineSize, dstChannels, dstSampleCount, dstSampleFormat, 1));
                }

                var ptrs = frame->data.ToArray();
                int convertRet;

                // Next, resample.
                fixed (byte** data = ptrs) {
                    convertRet = ffmpeg.swr_convert(resampleContext, dstData, dstSampleCount, data, frame->nb_samples);

                    Verify(convertRet);
                }

                // Get resampled data size...
                var resampledDataSize = ffmpeg.av_samples_get_buffer_size(&dstLineSize, dstChannels, convertRet, dstSampleFormat, 1);

                // ... allocates the buffer...
                buffer = new byte[resampledDataSize];

                // .. and write to it.
                using (var dest = new MemoryStream(buffer, true)) {
                    // TODO: sometimes dstData[0] is null?
                    if (dstData[0] != null) {
                        using (var src = new UnmanagedMemoryStream(dstData[0], resampledDataSize)) {
                            src.CopyTo(dest);
                        }
                    }
                }
            } finally {
                // Finally, clean up the native buffers.
                if (dstData != null) {
                    if (dstData[0] != null) {
                        ffmpeg.av_freep(&dstData[0]);
                    }

                    ffmpeg.av_freep(dstData);
                }
            }

            return true;
        }

    }
}
