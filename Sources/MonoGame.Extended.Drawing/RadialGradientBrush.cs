using System;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Drawing.Effects;

namespace MonoGame.Extended.Drawing;

[PublicAPI]
public sealed class RadialGradientBrush : Brush
{

    public RadialGradientBrush(DrawingContext context, RadialGradientBrushProperties properties, GradientStopCollection gradientStopCollection)
        : this(context, properties, BrushProperties.Default, gradientStopCollection)
    {
    }

    public RadialGradientBrush(DrawingContext context, RadialGradientBrushProperties properties, BrushProperties brushProperties, GradientStopCollection gradientStopCollection)
        : base(context, LoadEffect, brushProperties)
    {
        GradientStopCollection = gradientStopCollection;
        Properties = properties;
    }

    public GradientStopCollection GradientStopCollection { get; }

    public RadialGradientBrushProperties Properties { get; }

    protected override void RenderInternal(Triangle[] triangles, Effect effect, Matrix3x2? transform)
    {
        throw new NotImplementedException();
    }

    private static EffectLoadingResult LoadEffect(DrawingContext drawingContext)
    {
        throw new NotImplementedException();
    }

}
