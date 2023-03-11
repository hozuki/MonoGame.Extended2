using Microsoft.Xna.Framework;
using MonoGame.Extended.Overlay.Extensions;
using SkiaSharp;

namespace MonoGame.Extended.Overlay;

public sealed class SolidBrush : Brush
{

    public SolidBrush(Color color)
    {
        Color = color;

        var paint = new SKPaint();

        paint.Color = color.ToSKColor();
        paint.IsAntialias = true;
        paint.IsStroke = false;

        Paint = paint;
    }

    public Color Color { get; }

    internal override SKPaint Paint { get; }

}
