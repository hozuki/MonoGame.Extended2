using System;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Drawing.Effects;

namespace MonoGame.Extended.Drawing;

[PublicAPI]
public abstract class Brush : DisposableBase
{

    private protected Brush(DrawingContext context, Func<DrawingContext, EffectLoadingResult> effectLoaderFunc, BrushProperties brushProperties)
    {
        Guard.ArgumentNotNull(context, nameof(context));
        Guard.ArgumentNotNull(effectLoaderFunc, nameof(effectLoaderFunc));

        DrawingContext = context;
        BrushProperties = brushProperties;

        (_brushEffect, _isEffectShared) = effectLoaderFunc(context);
    }

    public BrushProperties BrushProperties { get; }

    internal void Render(Triangle[] triangles, Matrix3x2? transform)
    {
        if (triangles.Length > 0)
        {
            RenderInternal(triangles, _brushEffect, transform);
        }
    }

    internal DrawingContext DrawingContext { get; }

    protected abstract void RenderInternal(Triangle[] triangles, Effect effect, Matrix3x2? transform);

    protected override void Dispose(bool disposing)
    {
        if (!_isEffectShared)
        {
            if (disposing)
            {
                _brushEffect.Dispose();
            }
        }
    }

    protected static readonly RasterizerState DefaultBrushRasterizerState = new()
    {
        CullMode = CullMode.None,
        MultiSampleAntiAlias = true,
    };

    protected static readonly DepthStencilState DefaultBrushDepthStencilState = new()
    {
        DepthBufferEnable = false,
        DepthBufferFunction = CompareFunction.Always,
        DepthBufferWriteEnable = true,
    };

    private readonly Effect _brushEffect;
    private readonly bool _isEffectShared;

}
