using System;
using JetBrains.Annotations;

namespace MonoGame.Extended.Drawing;

[PublicAPI]
public sealed class Pen
{

    public Pen(Brush brush, float strokeWidth)
        : this(brush, strokeWidth, null)
    {
    }

    public Pen(Brush brush, float strokeWidth, StrokeStyle? strokeStyle)
    {
        if (strokeWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(strokeWidth), strokeWidth, "Stroke width must be greater than 0.");
        }

        if (strokeStyle is null)
        {
            strokeStyle = StrokeStyle.DefaultStyle;
        }

        StrokeWidth = strokeWidth;
        StrokeStyle = strokeStyle;
        Brush = brush;
    }

    public float StrokeWidth { get; }

    public StrokeStyle StrokeStyle { get; }

    internal Brush Brush { get; }

}
