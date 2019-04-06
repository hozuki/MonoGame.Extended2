using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Assassin;
using Assassin.Native;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Framework.Media;

namespace Demo.VideoPlayback.AssSubtitle {
    public class AssSubtitleRenderer : SubtitleRenderer {

        public AssSubtitleRenderer() {
            _library = AssLibrary.Create();
            _renderer = _library.CreateRenderer();

            _renderer.SetFonts("Arial");
        }

        public void LoadFromFile([NotNull] string fileName) {
            _currentTrack?.Dispose();

            var source = new FileAssSource(fileName);

            _currentTrack = _library.CreateTrack(source);
        }

        public void LoadFromString([NotNull] string content) {
            _currentTrack?.Dispose();

            var source = new StringAssSource(content);

            _currentTrack = _library.CreateTrack(source);
        }

        public override Point Dimensions {
            get => _dimensions;
            set {
                _dimensions = value;

                _renderer.SetFrameSize(value.X, value.Y);
                _imageBuffer = new RgbaImage(value.X, value.Y);
            }
        }

        public override void Render(TimeSpan time, RenderTarget2D texture) {
            if (!Enabled) {
                return;
            }

            if (_currentTrack == null) {
                // Subtitle is not loaded
                return;
            }

            if (_imageBuffer == null) {
                throw new InvalidOperationException("Image buffer is not created. Maybe dimensions are not set.");
            }

            _imageBuffer.Clear();

            var now = (long)Math.Round(time.TotalMilliseconds);
            var image = _renderer.RenderFrame(_currentTrack, now);

            image.Blend(_imageBuffer);

            DrawOnTexture(_imageBuffer, texture);
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                _currentTrack?.Dispose();
                _currentTrack = null;

                _renderer.Dispose();
                _library.Dispose();
            }
        }

        private void DrawOnTexture([NotNull] RgbaImage image, [NotNull] Texture2D texture) {
            Debug.Assert(texture.Format == SurfaceFormat.Color);
            Debug.Assert(image.Width == texture.Width);
            Debug.Assert(image.Height == texture.Height);

            var width = image.Width;
            var height = image.Height;

            var textureData = new Color[width * height];

            texture.GetData(textureData);

            unsafe {
                fixed (Color* dest = textureData) {
                    fixed (Color32* src = image.Buffer) {
                        for (var j = 0; j < height; ++j) {
                            for (var i = 0; i < width; ++i) {
                                var index = j * width + i;
                                var s = src[index];
                                var d = dest[index];
                                var blended = AlphaBlend(in s, in d);

                                dest[index] = blended;
                            }
                        }
                    }
                }
            }

            texture.SetData(textureData);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Color AlphaBlend(in Color32 src, in Color dst) {
            var alpha = src.A / (float)byte.MaxValue;
            var mk = 1 - alpha;

            var r = MathHelper.Clamp((int)(alpha * src.R + dst.R * mk), byte.MinValue, byte.MaxValue);
            var g = MathHelper.Clamp((int)(alpha * src.G + dst.G * mk), byte.MinValue, byte.MaxValue);
            var b = MathHelper.Clamp((int)(alpha * src.B + dst.B * mk), byte.MinValue, byte.MaxValue);
            var a = MathHelper.Clamp((int)(alpha * src.A + dst.A * mk), byte.MinValue, byte.MaxValue);

            return new Color {
                R = (byte)r,
                G = (byte)g,
                B = (byte)b,
                A = (byte)a
            };
        }

        [NotNull]
        private AssLibrary _library;

        [NotNull]
        private AssRenderer _renderer;

        [CanBeNull]
        private AssTrack _currentTrack;

        [CanBeNull]
        private RgbaImage _imageBuffer;

        private Point _dimensions;

    }
}
