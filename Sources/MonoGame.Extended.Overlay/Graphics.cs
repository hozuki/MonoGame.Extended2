using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Overlay.Extensions;
using SkiaSharp;

namespace MonoGame.Extended.Overlay;

public sealed class Graphics : DisposableBase
{

    /// <summary>
    /// Creates a flexible-sized <see cref="Graphics"/>. Its size is always the underlying <see cref="GraphicsDevice"/>'s size.
    /// </summary>
    /// <param name="graphicsDevice"></param>
    public Graphics(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;

        (_surface, _canvas, _backBuffer, _backBufferData, _bounds) = CreateResources(graphicsDevice, null);

        graphicsDevice.DeviceReset += GraphicsDevice_DeviceReset;
    }

    /// <summary>
    /// Creates a fixed-sized <see cref="Graphics"/>.
    /// </summary>
    /// <param name="graphicsDevice"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public Graphics(GraphicsDevice graphicsDevice, int width, int height)
    {
        Guard.GreaterThan(width, 0, nameof(width));
        Guard.GreaterThan(height, 0, nameof(height));

        _graphicsDevice = graphicsDevice;

        var customBounds = new Rectangle(0, 0, width, height);
        _isCustomSize = true;

        (_surface, _canvas, _backBuffer, _backBufferData, _bounds) = CreateResources(graphicsDevice, customBounds);

        graphicsDevice.DeviceReset += GraphicsDevice_DeviceReset;
    }

    public event EventHandler<EventArgs>? ContentChanged;

    public Rectangle Bounds => _bounds;

    public Texture2D BackBuffer => _backBuffer;

    public bool UpdateBackBuffer()
    {
        EnsureNotDisposed();

        if (!_isDirty)
        {
            return true;
        }

        var viewport = _graphicsDevice.Viewport;
        var destInfo = new SKImageInfo(viewport.Width, viewport.Height, SKColorType.Rgba8888);
        bool successful;

        unsafe
        {
            fixed (byte* bufferPtr = _backBufferData)
            {
                var ptr = new IntPtr(bufferPtr);

                successful = _surface.ReadPixels(destInfo, ptr, viewport.Width * sizeof(int), 0, 0);
            }
        }

        if (successful)
        {
            _backBuffer.SetData(_backBufferData);
            _isDirty = false;
        }

        return successful;
    }

    public void Resize(int width, int height)
    {
        Guard.GreaterThan(width, 0, nameof(width));
        Guard.GreaterThan(height, 0, nameof(height));

        _isCustomSize = true;

        RecreateResources();
    }

    public void Clear(Color color)
    {
        _canvas.Clear(color.ToSKColor());
        SetDirty();
    }

    public void DrawLine(Pen pen, float x1, float y1, float x2, float y2)
    {
        _canvas.DrawLine(x1, y1, x2, y2, pen.Paint);
        SetDirty();
    }

    public void DrawLine(Pen pen, Vector2 p1, Vector2 p2)
    {
        DrawLine(pen, p1.X, p1.Y, p2.X, p2.Y);
    }

    public void DrawRectangle(Pen pen, float x, float y, float width, float height)
    {
        DrawOrFillRectangle(pen, x, y, width, height);
    }

    public void DrawRectangle(Pen pen, Rectangle rect)
    {
        DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
    }

    public void FillRectangle(Brush brush, float x, float y, float width, float height)
    {
        DrawOrFillRectangle(brush, x, y, width, height);
    }

    public void FillRectangle(Brush brush, Rectangle rect)
    {
        FillRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height);
    }

    public void DrawEllipse(Pen pen, float x, float y, float width, float height)
    {
        DrawOrFillEllipse(pen, x, y, width, height);
    }

    public void DrawEllipse(Pen pen, Rectangle rect)
    {
        DrawEllipse(pen, rect.X, rect.Y, rect.Width, rect.Height);
    }

    public void FillEllipse(Brush brush, float x, float y, float width, float height)
    {
        DrawOrFillEllipse(brush, x, y, width, height);
    }

    public void FillEllipse(Brush brush, Rectangle rect)
    {
        FillEllipse(brush, rect.X, rect.Y, rect.Width, rect.Height);
    }

    public void DrawPolygon(Pen pen, Vector2[] points)
    {
        DrawOrFillPolygon(pen, points);
    }

    public void FillPolygon(Brush brush, Vector2[] points)
    {
        DrawOrFillPolygon(brush, points);
    }

    public void DrawString(Pen pen, Font font, string? str, Vector2 position, StringFormat? stringFormat = null)
    {
        DrawString(pen, font, str, position.X, position.Y, stringFormat);
    }

    public void DrawString(Pen pen, Font font, string? str, float x, float y, StringFormat? stringFormat = null)
    {
        DrawOrFillString(pen, font, str, x, y, stringFormat);
    }

    public void FillString(Brush brush, Font font, string? str, Vector2 position, StringFormat? stringFormat = null)
    {
        FillString(brush, font, str, position.X, position.Y, stringFormat);
    }

    public void FillString(Brush brush, Font font, string? str, float x, float y, StringFormat? stringFormat = null)
    {
        DrawOrFillString(brush, font, str, x, y, stringFormat);
    }

    public Vector2 MeasureString(Font font, string? str, StringFormat? stringFormat = null)
    {
        return MeasureString(font, str, new Vector2(float.MaxValue, float.MaxValue), stringFormat);
    }

    public Vector2 MeasureString(Font font, string? str, Vector2 maxBounds, StringFormat? stringFormat = null)
    {
        var skBounds = new SKRect(0, 0, maxBounds.X, maxBounds.Y);

        using (var paint = new SKPaint())
        {
            SetSKPaintFontProperties(paint, font, stringFormat);
            paint.MeasureText(str, ref skBounds);
        }

        return new Vector2(skBounds.Width, skBounds.Height);
    }

    public void DrawMultilineString(Pen pen, Font font, string? str, Vector2 position, StringFormat? stringFormat = null)
    {
        DrawMultilineString(pen, font, str, position, new Vector2(float.MaxValue, float.MaxValue), stringFormat);
    }

    public void DrawMultilineString(Pen pen, Font font, string? str, Vector2 position, Vector2 maxBounds, StringFormat? stringFormat = null)
    {
        DrawMultilineString(pen, font, str, position.X, position.Y, maxBounds.X, maxBounds.Y, stringFormat);
    }

    public void DrawMultilineString(Pen pen, Font font, string? str, float x, float y, StringFormat? stringFormat = null)
    {
        DrawOrFillMultilineString(pen, font, str, x, y, float.MaxValue, float.MaxValue, stringFormat);
    }

    public void DrawMultilineString(Pen pen, Font font, string? str, float x, float y, float maxWidth, float maxHeight, StringFormat? stringFormat = null)
    {
        DrawOrFillMultilineString(pen, font, str, x, y, maxWidth, maxHeight, stringFormat);
    }

    public void FillMultilineString(Brush brush, Font font, string? str, Vector2 position, StringFormat? stringFormat = null)
    {
        FillMultilineString(brush, font, str, position, new Vector2(float.MaxValue, float.MaxValue), stringFormat);
    }

    public void FillMultilineString(Brush brush, Font font, string? str, Vector2 position, Vector2 maxBounds, StringFormat? stringFormat = null)
    {
        FillMultilineString(brush, font, str, position.X, position.Y, maxBounds.X, maxBounds.Y, stringFormat);
    }

    public void FillMultilineString(Brush brush, Font font, string? str, float x, float y, StringFormat? stringFormat = null)
    {
        DrawOrFillMultilineString(brush, font, str, x, y, float.MaxValue, float.MaxValue, stringFormat);
    }

    public void FillMultilineString(Brush brush, Font font, string? str, float x, float y, float maxWidth, float maxHeight, StringFormat? stringFormat = null)
    {
        DrawOrFillMultilineString(brush, font, str, x, y, maxWidth, maxHeight, stringFormat);
    }

    public Vector2 MeasureMultilineString(Font font, string? str, StringFormat? stringFormat = null)
    {
        return MeasureMultilineString(font, str, new Vector2(float.MaxValue, float.MaxValue), stringFormat);
    }

    public Vector2 MeasureMultilineString(Font font, string? str, Vector2 maxBounds, StringFormat? stringFormat = null)
    {
        if (string.IsNullOrEmpty(str))
        {
            return Vector2.Zero;
        }

        using var paint = new SKPaint();

        SetSKPaintFontProperties(paint, font, stringFormat);

        var lines = SplitLines(str, paint, maxBounds.X, maxBounds.Y, stringFormat);
        var lineHeight = DetermineLineHeight(paint, stringFormat);

        var width = lines.Max(line => line.Width);
        var height = lineHeight * lines.Length;

        return new Vector2(width, height);
    }

    public void DrawMesh(Pen pen, Triangle[] triangles)
    {
        DrawOrFillMesh(pen, triangles);
    }

    public void FillMesh(Brush brush, Triangle[] triangles)
    {
        DrawOrFillMesh(brush, triangles);
    }

    public void DrawImage(Texture2D texture, Vector2 position, bool antiAliased = true)
    {
        DrawImage(texture, position.X, position.Y, texture.Width, texture.Height, antiAliased);
    }

    public void DrawImage(Texture2D texture, float x, float y, bool antiAliased = true)
    {
        DrawImage(texture, x, y, texture.Width, texture.Height, antiAliased);
    }

    public void DrawImage(Texture2D texture, Rectangle destRect, bool antiAliased = true)
    {
        DrawImage(texture, destRect.X, destRect.Y, destRect.Width, destRect.Height, antiAliased);
    }

    public void DrawImage(Texture2D texture, float x, float y, float width, float height, bool antiAliased = true)
    {
        var buffer = new byte[texture.Width * texture.Height * sizeof(int)];

        texture.GetData(buffer);

        DrawImage(buffer, texture.Format, texture.Width, texture.Height, x, y, width, height, antiAliased);
    }

    public void DrawImage(byte[] textureData, SurfaceFormat textureFormat, int textureWidth, int textureHeight, float x, float y, float width, float height, bool antiAliased = true)
    {
        Debug.Assert(textureFormat is SurfaceFormat.Color, "textureFormat is SurfaceFormat.Color");

        using var paint = new SKPaint();

        paint.IsAntialias = antiAliased;
        paint.HintingLevel = SKPaintHinting.Full;
        paint.IsAutohinted = true;

        var imageInfo = new SKImageInfo(textureWidth, textureHeight, SKColorType.Rgba8888, SKAlphaType.Premul);

        unsafe
        {
            fixed (byte* bufferPtr = textureData)
            {
                var ptr = new IntPtr(bufferPtr);

                using var image = SKImage.FromPixels(imageInfo, ptr, textureWidth * sizeof(int));

                var destRect = new SKRect(x, y, x + width, y + height);
                _canvas.DrawImage(image, destRect, paint);
            }
        }
    }

    public void SetClipPath(Path clipPath, bool antiAliased)
    {
        _canvas.ClipPath(clipPath.NativePath, antialias: antiAliased);
    }

    public int SaveState()
    {
        return _canvas.Save();
    }

    public void RestoreState()
    {
        _canvas.Restore();
    }

    protected override void Dispose(bool disposing)
    {
        _graphicsDevice.DeviceReset -= GraphicsDevice_DeviceReset;

        if (disposing)
        {
            DisposeResources();
        }
    }

    private void DrawOrFillRectangle(IPaintProvider provider, float x, float y, float width, float height)
    {
        _canvas.DrawRect(new SKRect(x, y, x + width, y + height), provider.Paint);
        SetDirty();
    }

    private void DrawOrFillEllipse(IPaintProvider provider, float x, float y, float width, float height)
    {
        _canvas.DrawOval(new SKRect(x, y, x + width, y + height), provider.Paint);
        SetDirty();
    }

    private void DrawOrFillPolygon(IPaintProvider provider, Vector2[] points)
    {
        Guard.ArgumentNotNull(points, nameof(points));

        if (points.Length < 3)
        {
            throw new ArgumentException("A polygon should contain at least 3 points.", nameof(points));
        }

        using (var path = new SKPath())
        {
            var skPoints = Array.ConvertAll(points, XnaExtensions.ToSKPoint);

            path.AddPoly(skPoints);
            _canvas.DrawPath(path, provider.Paint);
        }

        SetDirty();
    }

    private void DrawOrFillString(IPaintProvider provider, Font font, string? str, float x, float y, StringFormat? stringFormat)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return;
        }

        using (var paint = provider.Paint.Clone())
        {
            SetSKPaintFontProperties(paint, font, stringFormat);
            _canvas.DrawText(str, x, y, paint);
        }

        SetDirty();
    }

    private void DrawOrFillMultilineString(IPaintProvider provider, Font font, string? str, float x, float y, float maxWidth, float maxHeight, StringFormat? stringFormat)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return;
        }

        using (var paint = provider.Paint.Clone())
        {
            SetSKPaintFontProperties(paint, font, stringFormat);

            var lines = SplitLines(str, paint, maxWidth, maxHeight, stringFormat);
            var lineHeight = DetermineLineHeight(paint, stringFormat);

            foreach (var line in lines)
            {
                _canvas.DrawText(line.Value, x, y, paint);

                y += lineHeight;
            }
        }

        SetDirty();
    }

    private void DrawOrFillMesh(IPaintProvider provider, Triangle[] mesh)
    {
        using (var paint = provider.Paint.Clone())
        {
            const SKVertexMode geometryMode = SKVertexMode.Triangles;

            var vertices = new SKPoint[mesh.Length * 3];

            for (var i = 0; i < mesh.Length; ++i)
            {
                var j = i * 3;

                vertices[j] = mesh[i].Point1.ToSKPoint();
                vertices[j + 1] = mesh[i].Point2.ToSKPoint();
                vertices[j + 2] = mesh[i].Point3.ToSKPoint();
            }

            var colors = new SKColor[vertices.Length];

            for (var i = 0; i < colors.Length; ++i)
            {
                colors[i] = SKColors.White;
            }

            _canvas.DrawVertices(geometryMode, vertices, colors, paint);
        }

        SetDirty();
    }

    private void RecreateResources()
    {
        DisposeResources();

        Rectangle? customBounds = null;

        if (_isCustomSize)
        {
            customBounds = _bounds;
        }

        (_surface, _canvas, _backBuffer, _backBufferData, _bounds) = CreateResources(_graphicsDevice, customBounds);
    }

    private void DisposeResources()
    {
        _canvas.Dispose();
        _surface.Dispose();
        _backBuffer.Dispose();
    }

    private static (SKSurface Surface, SKCanvas Canvas, Texture2D BackBuffer, byte[] BackBufferData, Rectangle Bounds) CreateResources(GraphicsDevice graphicsDevice, Rectangle? customBounds)
    {
        Rectangle bounds;

        if (customBounds.HasValue)
        {
            bounds = customBounds.Value;
        }
        else
        {
            var viewport = graphicsDevice.Viewport;
            bounds = viewport.Bounds;
        }

        var surface = SKSurface.Create(bounds.Width, bounds.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
        var canvas = surface.Canvas;

        var backBuffer = new Texture2D(graphicsDevice, bounds.Width, bounds.Height, false, SurfaceFormat.Color);
        var backBufferData = new byte[bounds.Width * bounds.Height * sizeof(int)];

        return (surface, canvas, backBuffer, backBufferData, bounds);
    }

    // ReSharper disable once InconsistentNaming
    private static void SetSKPaintFontProperties(SKPaint paint, Font font, StringFormat? stringFormat)
    {
        paint.Typeface = font.Typeface;
        paint.TextSize = font.Size;
        paint.FakeBoldText = font.FakeBold;

        paint.SubpixelText = true;

        if (stringFormat is not null)
        {
            paint.IsVerticalText = stringFormat.IsVertical;
            paint.TextAlign = (SKTextAlign)stringFormat.Align;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetDirty()
    {
        _isDirty = true;
    }

    private void GraphicsDevice_DeviceReset(object? sender, EventArgs e)
    {
        RecreateResources();
        SetDirty();
        ContentChanged?.Invoke(this, EventArgs.Empty);
    }

    private static StringLine[] SplitLines(string text, SKPaint paint, float maxWidth, float maxHeight, StringFormat? stringFormat)
    {
        // TODO: Some critical layout calculations are not done in this method:
        // 1. vertical text;
        // 2. spaces at the end of line

        var lines = text.Split(LfLineSeparator);
        var lineHeight = DetermineLineHeight(paint, stringFormat);

        var result = new List<StringLine>();
        var height = lineHeight;

        if (height > maxHeight)
        {
            return result.ToArray();
        }

        var lineResult = new StringBuilder();

        foreach (var line in lines)
        {
            float width = 0;

            foreach (var ch in line)
            {
                var charWidth = paint.MeasureText(ch.ToString());

                if (width + charWidth >= maxWidth)
                {
                    var stringLine = new StringLine(lineResult.ToString(), width);

                    result.Add(stringLine);

                    height += lineHeight;

                    if (height > maxHeight)
                    {
                        return result.ToArray();
                    }

                    lineResult.Clear();
                    width = 0;
                }
                else
                {
                    lineResult.Append(ch);
                    width += charWidth;
                }
            }

            if (lineResult.Length > 0)
            {
                var stringLine = new StringLine(lineResult.ToString(), width);

                result.Add(stringLine);

                lineResult.Clear();
            }
        }

        return result.ToArray();
    }

    private static float DetermineLineHeight(SKPaint paint, StringFormat? stringFormat)
    {
        float lineHeight;

        if (stringFormat?.PreferredLineHeight != null)
        {
            lineHeight = stringFormat.PreferredLineHeight.Value;

            if (lineHeight <= 0)
            {
                lineHeight = 15.0f;
            }
        }
        else
        {
            lineHeight = paint.FontSpacing;
        }

        return lineHeight;
    }

    private static readonly char[] LfLineSeparator = { '\n' };

    private readonly GraphicsDevice _graphicsDevice;

    private Texture2D _backBuffer;

    private byte[] _backBufferData;

    private SKSurface _surface;
    private SKCanvas _canvas;

    private bool _isCustomSize;
    private Rectangle _bounds;

    private bool _isDirty;

}
