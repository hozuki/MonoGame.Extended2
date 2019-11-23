using System;
using System.Diagnostics;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended.VideoPlayback;
using ThreadState = System.Threading.ThreadState;

// ReSharper disable once CheckNamespace
namespace MonoGame.Extended.Framework.Media {
    partial class VideoPlayer {

        /// <summary>
        /// The decoding thread of a <see cref="VideoPlayer"/>.
        /// </summary>
        private sealed class DecodingThread {

            /// <summary>
            /// Creates a new <see cref="DecodingThread"/> instance.
            /// </summary>
            /// <param name="videoPlayer">The parent <see cref="VideoPlayer"/>.</param>
            /// <param name="playerOptions">Player options.</param>
            internal DecodingThread([NotNull] VideoPlayer videoPlayer, [NotNull] VideoPlayerOptions playerOptions) {
                _videoPlayer = videoPlayer;
                _playerOptions = playerOptions;

                // We need SynchronizationContext to post messages for us.
                // And gracefully, SynchronizationContext.Current is thread-local.
                var syncContext = SynchronizationContext.Current;

                if (syncContext == null) {
                    syncContext = new SynchronizationContext();
                    SynchronizationContext.SetSynchronizationContext(syncContext);
                }

                _mainThreadSynchronizationContext = syncContext;

                var thread = new Thread(ThreadProc);

                thread.IsBackground = true;
                SystemThread = thread;
            }

            /// <summary>
            /// Starts the underlying <see cref="Thread"/>.
            /// </summary>
            internal void Start() {
                if ((SystemThread.ThreadState & ThreadState.Unstarted) != 0) {
                    SystemThread.Start();
                }
            }

            /// <summary>
            /// Sends a termination signal to the underlying <see cref="Thread"/>, and waits for it to exit.
            /// </summary>
            internal void Terminate() {
                _continueWorking = false;

                if (SystemThread.IsAlive) {
                    SystemThread.Join();
                }
            }

            /// <summary>
            /// The underlying <see cref="Thread"/>.
            /// </summary>
            [NotNull]
            internal Thread SystemThread { get; }

            /// <summary>
            /// Returns <see langword="null"/> if the thread is still running, <see langword="true"/> if the thread exited abnormally,
            /// and <see langword="false"/> if the thread exited normally.
            /// </summary>
            [CanBeNull]
            internal bool? ExceptionalExit => _exceptionalExit;

#if DEBUG
            /// <summary>
            /// When the thread surprisingly exited, retrieves the cause.
            /// </summary>
            [CanBeNull]
            internal Exception ExitCause {
                [DebuggerStepThrough]
                get => _exitCause;
            }
#endif

            /// <summary>
            /// Worker thread procedure.
            /// </summary>
            private void ThreadProc() {
                try {
                    var videoPlayer = _videoPlayer;
                    var video = videoPlayer.Video;
                    var interval = _playerOptions.DecodingThreadSleepInterval;

                    while (_continueWorking) {
                        switch (videoPlayer.State) {
                            case MediaState.Paused:
                                break;
                            case MediaState.Stopped:
                                break;
                            case MediaState.Playing:
                                var currentTime = videoPlayer.PlayPosition;
                                var presentationTime = currentTime.TotalSeconds;

                                var decodeContext = video.DecodeContext;

                                if (decodeContext != null) {
                                    // 2018-01-30:
                                    // Seems like we should decode and push audio data first, rather than video data first.
                                    // The former way produces less lag.
                                    decodeContext.LockFrameQueuesUpdate();

                                    try {
                                        var soundEffect = videoPlayer._soundEffectInstance;

                                        if (soundEffect != null) {
                                            decodeContext.ReadAudioUntilPlaybackIsAfter(soundEffect, presentationTime);
                                        }

                                        decodeContext.ReadVideoUntilPlaybackIsAfter(presentationTime);
                                    } finally {
                                        decodeContext.UnlockFrameQueueUpdate();
                                    }
                                }

                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        Thread.Sleep(interval);
                    }

                    video.DecodeContext?.Reset();

                    _exceptionalExit = false;
                } catch (Exception ex) {
                    _exceptionalExit = true;
#if DEBUG
                    _exitCause = ex;
#endif

                    // Here is a trick to raise an exception from a worker thread with a correct stack trace.
                    // First, the exception must be raised in the main thread, otherwise the application crashes immediately.
                    // SynchronizationContext.Post() is used to make it actually happen in the main thread, not in the worker thread.
                    // Second, if the delegate is simply "_ => throw ex", the stack trace is not preserved.
                    // The source location is set to the SynchronizationContext.Post() call, which is useless for debugging.
                    // So we have to wrap another exception, e.g. ApplicationException.
                    // And then when the ApplicationException in the main thread is caught (see AppDomain.UnhandledException event),
                    // use its InnerException property to get the real exception, and the correct source location.
                    _mainThreadSynchronizationContext.Post(_ => throw new ApplicationException(ex.Message, ex), null);
                }
            }

            [CanBeNull]
            private bool? _exceptionalExit;

#if DEBUG
            [CanBeNull]
            private Exception _exitCause;
#endif

            [NotNull]
            private readonly SynchronizationContext _mainThreadSynchronizationContext;

            private bool _continueWorking = true;

            [NotNull]
            private readonly VideoPlayer _videoPlayer;

            [NotNull]
            private readonly VideoPlayerOptions _playerOptions;

        }

    }
}
