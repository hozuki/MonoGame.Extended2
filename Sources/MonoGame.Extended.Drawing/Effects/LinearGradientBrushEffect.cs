using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.Drawing.Effects;

internal sealed class LinearGradientBrushEffect : GradientBrushEffect
{

    private LinearGradientBrushEffect(GraphicsDevice graphicsDevice, byte[] effectCode)
        : base(graphicsDevice, effectCode)
    {
        Initialize(Parameters, out _startPoint, out _endPoint);
    }

    public override void Apply()
    {
        var key = (Gamma, ExtendMode);
        var passName = PassNames[key];

        CurrentTechnique.Passes[passName].Apply();
    }

    public static LinearGradientBrushEffect Create(DrawingContext drawingContext)
    {
        var bytecode = drawingContext.EffectResources.LinearGradientBrush.Bytecode;

        return new LinearGradientBrushEffect(drawingContext.GraphicsDevice, bytecode);
    }

    public Vector2 StartPoint
    {
        get => _startPoint.GetValueVector2();
        set => _startPoint.SetValue(value);
    }

    public Vector2 EndPoint
    {
        get => _endPoint.GetValueVector2();
        set => _endPoint.SetValue(value);
    }

    private static void Initialize(EffectParameterCollection parameters, out EffectParameter startPoint, out EffectParameter endPoint)
    {
        startPoint = parameters["startPoint"];
        endPoint = parameters["endPoint"];
    }

    private static readonly IReadOnlyDictionary<(Gamma, ExtendMode), string> PassNames = new Dictionary<(Gamma, ExtendMode), string>
    {
        [(Gamma.SRgb, ExtendMode.Clamp)] = "SRgb_Clamp",
        [(Gamma.Linear, ExtendMode.Clamp)] = "Linear_Clamp",
        [(Gamma.SRgb, ExtendMode.Wrap)] = "SRgb_Wrap",
        [(Gamma.Linear, ExtendMode.Wrap)] = "Linear_Wrap",
        [(Gamma.SRgb, ExtendMode.Mirror)] = "SRgb_Mirror",
        [(Gamma.Linear, ExtendMode.Mirror)] = "Linear_Mirror",
    };

    private readonly EffectParameter _startPoint;
    private readonly EffectParameter _endPoint;

}
