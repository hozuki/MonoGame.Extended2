using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Overlay.Extensions;
using SkiaSharp;

namespace MonoGame.Extended.Overlay {
    public sealed class Graphics : DisposableBase {

        /// <summary>
        /// Creates a flexible-sized <see cref="Graphics"/>. Its size is always the underlying <see cref="GraphicsDevice"/>'s size.
        /// </summary>
        /// <param name="graphicsDevice"></param>
        public Graphics([NotNull] GraphicsDevice graphicsDevice) {
            _graphicsDevice = graphicsDevice;
            _bounds = graphicsDevice.Viewport.Bounds;

            RecreateResources();

            graphicsDevice.DeviceReset += GraphicsDevice_DeviceReset;
        }

        /// <summary>
        /// Creates a fixed-sized <see cref="Graphics"/>.
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public Graphics([NotNull] GraphicsDevice graphicsDevice, int width, int height) {
            Guard.GreaterThan(width, 0, nameof(width));
            Guard.GreaterThan(height, 0, nameof(height));

            _graphicsDevice = graphicsDevice;
            _bounds = new Rectangle(0, 0, width, height);
            _isCustomSize = true;

            RecreateResources();

            graphicsDevice.DeviceReset += GraphicsDevice_DeviceReset;
        }

        public event EventHandler<EventArgs> ContentChanged;

        public Rectangle Bounds => _bounds;

        public Texture2D BackBuffer => _backBuffer;

        public bool UpdateBackBuffer() {
            EnsureNotDisposed();

            if (!_isDirty) {
                return true;
            }

            if (_surface == null) {
                return false;
            }

            var viewport = _graphicsDevice.Viewport;
            var destInfo = new SKImageInfo(viewport.Width, viewport.Height, SKColorType.Rgba8888);
            bool successful;

            unsafe {
                fixed (byte* bufferPtr = _backBufferData) {
                    var ptr = new IntPtr(bufferPtr);

                    successful = _surface.ReadPixels(destInfo, ptr, viewport.Width * sizeof(int), 0, 0);
                }
            }

            if (successful) {
                _backBuffer.SetData(_backBufferData);
                _isDirty = false;
            }

            return successful;
        }

        public void Clear(Color color) {
            _canvas?.Clear(color.ToSKColor());
            SetDirty();
        }

        public void DrawLine([NotNull] Pen pen, float x1, float y1, float x2, float y2) {
            _canvas?.DrawLine(x1, y1, x2, y2, pen.Paint);
            SetDirty();
        }

        public void DrawLine([NotNull] Pen pen, Vector2 p1, Vector2 p2) {
            DrawLine(pen, p1.X, p1.Y, p2.X, p2.Y);
        }

        public void DrawRectangle([NotNull] Pen pen, float x, float y, float width, float height) {
            DrawOrFillRectangle(pen, x, y, width, height);
        }

        public void DrawRectangle([NotNull] Pen pen, Rectangle rect) {
            DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void FillRectangle([NotNull] Brush brush, float x, float y, float width, float height) {
            DrawOrFillRectangle(brush, x, y, width, height);
        }

        public void FillRectangle([NotNull] Brush brush, Rectangle rect) {
            FillRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void DrawEllipse([NotNull] Pen pen, float x, float y, float width, float height) {
            DrawOrFillEllipse(pen, x, y, width, height);
        }

        public void DrawEllipse([NotNull] Pen pen, Rectangle rect) {
            DrawEllipse(pen, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void FillEllipse([NotNull] Brush brush, float x, float y, float width, float height) {
            DrawOrFillEllipse(brush, x, y, width, height);
        }

        public void FillEllipse([NotNull] Brush brush, Rectangle rect) {
            FillEllipse(brush, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void DrawEllipse([NotNull] Pen pen, [NotNull] Vector2[] points) {
            DrawOrFillPolygon(pen, points);
        }

        public void FillEllipse([NotNull] Brush brush, [NotNull] Vector2[] points) {
            DrawOrFillPolygon(brush, points);
        }

        public void DrawString([NotNull] Pen pen, [NotNull] Font font, [CanBeNull] string str, Vector2 position, [CanBeNull] StringFormat stringFormat = null) {
            DrawString(pen, font, str, position.X, position.Y, stringFormat);
        }

        public void DrawString([NotNull] Pen pen, [NotNull] Font font, [CanBeNull] string str, float x, float y, [CanBeNull] StringFormat stringFormat = null) {
            DrawOrFillString(pen, font, str, x, y, stringFormat);
        }

        public void FillString([NotNull] Brush brush, [NotNull] Font font, [CanBeNull] string str, Vector2 position, [CanBeNull] StringFormat stringFormat = null) {
            FillString(brush, font, str, position.X, position.Y, stringFormat);
        }

        public void FillString([NotNull] Brush brush, [NotNull] Font font, [CanBeNull] string str, float x, float y, [CanBeNull] StringFormat stringFormat = null) {
            DrawOrFillString(brush, font, str, x, y, stringFormat);
        }

        public Vector2 MeasureString([NotNull] Font font, [CanBeNull] string str, [CanBeNull] StringFormat stringFormat = null) {
            return MeasureString(font, str, new Vector2(float.MaxValue, float.MaxValue), stringFormat);
        }

        public Vector2 MeasureString([NotNull] Font font, [CanBeNull] string str, Vector2 maxBounds, [CanBeNull] StringFormat stringFormat = null) {
            var skBounds = new SKRect(0, 0, maxBounds.X, maxBounds.Y);

            using (var paint = new SKPaint()) {
                SetSKPaintFontProperties(paint, font, stringFormat);
                paint.MeasureText(str, ref skBounds);
            }

            return new Vector2(skBounds.Width, skBounds.Height);
        }

        public void DrawImage([NotNull] Texture2D texture, Vector2 position, bool antialiased = true) {
            DrawImage(texture, position.X, position.Y, texture.Width, texture.Height, antialiased);
        }

        public void DrawImage([NotNull] Texture2D texture, float x, float y, bool antialiased = true) {
            DrawImage(texture, x, y, texture.Width, texture.Height, antialiased);
        }

        public void DrawImage([NotNull] Texture2D texture, Rectangle destRect, bool antialiased = true) {
            DrawImage(texture, destRect.X, destRect.Y, destRect.Width, destRect.Height, antialiased);
        }

        public void DrawImage([NotNull] Texture2D texture, float x, float y, float width, float height, bool antialiased = true) {
            var buffer = new byte[texture.Width * texture.Height * sizeof(int)];

            texture.GetData(buffer);

            DrawImage(buffer, texture.Format, texture.Width, texture.Height, x, y, width, height, antialiased);
        }

        public void DrawImage([NotNull] byte[] textureData, SurfaceFormat textureFormat, int textureWidth, int textureHeight, float x, float y, float width, float height, bool antialiased = true) {
            if (_canvas == null) {
                return;
            }

            Debug.Assert(textureFormat == SurfaceFormat.Color);

            using (var paint = new SKPaint()) {
                paint.IsAntialias = antialiased;
                paint.HintingLevel = SKPaintHinting.Full;
                paint.IsAutohinted = true;

                var imageInfo = new SKImageInfo(textureWidth, textureHeight, SKColorType.Rgba8888, SKAlphaType.Premul);

                unsafe {
                    fixed (byte* bufferPtr = textureData) {
                        var ptr = new IntPtr(bufferPtr);

                        using (var image = SKImage.FromPixels(imageInfo, ptr, textureWidth * sizeof(int))) {
                            var destRect = new SKRect(x, y, x + width, y + height);
                            _canvas.DrawImage(image, destRect, paint);
                        }
                    }
                }
            }
        }

        public void SetClipPath([NotNull] Path clipPath, bool antialiased) {
            _canvas?.ClipPath(clipPath.NativePath, antialias: antialiased);
        }

        public int SaveState() {
            return _canvas?.Save() ?? -1;
        }

        public void RestoreState() {
            _canvas?.Restore();
        }

        protected override void Dispose(bool disposing) {
            _graphicsDevice.DeviceReset -= GraphicsDevice_DeviceReset;
            _canvas = null;
            _surface?.Dispose();
            _surface = null;
        }

        private void DrawOrFillRectangle([NotNull] IPaintProvider provider, float x, float y, float width, float height) {
            _canvas?.DrawRect(new SKRect(x, y, x + width, y + height), provider.Paint);
            SetDirty();
        }

        private void DrawOrFillEllipse([NotNull] IPaintProvider provider, float x, float y, float width, float height) {
            _canvas?.DrawOval(new SKRect(x, y, x + width, y + height), provider.Paint);
            SetDirty();
        }

        private void DrawOrFillPolygon([NotNull] IPaintProvider provider, [NotNull] Vector2[] points) {
            Guard.ArgumentNotNull(points, nameof(points));

            if (points.Length < 3) {
                throw new ArgumentException("A polygon should contain at least 3 points.", nameof(points));
            }

            if (_canvas == null) {
                return;
            }

            using (var path = new SKPath()) {
                var skPoints = Array.ConvertAll(points, XnaExtensions.ToSKPoint);

                path.AddPoly(skPoints);
                _canvas.DrawPath(path, provider.Paint);
            }

            SetDirty();
        }

        private void DrawOrFillString([NotNull] IPaintProvider provider, [NotNull] Font font, [CanBeNull] string str, float x, float y, [CanBeNull] StringFormat stringFormat) {
            if (string.IsNullOrWhiteSpace(str)) {
                return;
            }

            using (var paint = provider.Paint.Clone()) {
                SetSKPaintFontProperties(paint, font, stringFormat);
                _canvas?.DrawText(str, x, y, paint);
            }

            SetDirty();
        }

        private void RecreateResources() {
            _canvas?.Dispose();
            _canvas = null;
            _surface?.Dispose();
            _surface = null;
            _backBuffer?.Dispose();

            if (!_isCustomSize) {
                var viewport = _graphicsDevice.Viewport;
                _bounds = viewport.Bounds;
            }

            var bounds = _bounds;

            _surface = SKSurface.Create(bounds.Width, bounds.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            _canvas = _surface?.Canvas;

            _backBuffer = new Texture2D(_graphicsDevice, bounds.Width, bounds.Height, false, SurfaceFormat.Color);
            _backBufferData = new byte[bounds.Width * bounds.Height * sizeof(int)];
        }

        // ReSharper disable once InconsistentNaming
        private static void SetSKPaintFontProperties([NotNull] SKPaint paint, [NotNull] Font font, [CanBeNull] StringFormat stringFormat) {
            paint.Typeface = font.Typeface;
            paint.TextSize = font.Size;
            paint.FakeBoldText = font.FakeBold;

            paint.SubpixelText = true;

            if (stringFormat != null) {
                paint.IsVerticalText = stringFormat.IsVertical;
                paint.TextAlign = (SKTextAlign)stringFormat.Align;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetDirty() {
            _isDirty = true;
        }

        private void GraphicsDevice_DeviceReset(object sender, EventArgs e) {
            RecreateResources();
            SetDirty();
            ContentChanged?.Invoke(this, EventArgs.Empty);
        }

        private readonly GraphicsDevice _graphicsDevice;

        private Texture2D _backBuffer;

        private byte[] _backBufferData;

        [CanBeNull]
        private SKSurface _surface;
        [CanBeNull]
        private SKCanvas _canvas;

        private bool _isCustomSize;
        private Rectangle _bounds;

        private bool _isDirty;

    }
}
