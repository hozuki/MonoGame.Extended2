using System;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Drawing.Effects;

namespace MonoGame.Extended.Drawing;

[PublicAPI]
public sealed class BitmapBrush : Brush
{

    public BitmapBrush(DrawingContext context, Texture2D bitmap, BitmapBrushProperties properties)
        : this(context, bitmap, properties, BrushProperties.Default)
    {
    }

    public BitmapBrush(DrawingContext context, Texture2D bitmap, BitmapBrushProperties properties, BrushProperties brushProperties)
        : base(context, LoadEffect, brushProperties)
    {
        Bitmap = bitmap;
        Properties = properties;
    }

    public Texture2D Bitmap { get; }

    public BitmapBrushProperties Properties { get; }

    protected override void RenderInternal(Triangle[] triangles, Effect effect, Matrix3x2? transform)
    {
        throw new NotImplementedException();
    }

    private static EffectLoadingResult LoadEffect(DrawingContext drawingContext)
    {
        throw new NotImplementedException();
    }

}
