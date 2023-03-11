using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.Drawing.Effects;

internal sealed class SolidColorBrushEffect : BrushEffect
{

    private SolidColorBrushEffect(GraphicsDevice graphicsDevice, byte[] effectCode)
        : base(graphicsDevice, effectCode)
    {
        Initialize(Parameters, out _color);
    }

    public override void Apply()
    {
        CurrentTechnique.Passes[0].Apply();
    }

    public static SolidColorBrushEffect Create(DrawingContext drawingContext)
    {
        var bytecode = drawingContext.EffectResources.SolidColorBrush.Bytecode;

        return new SolidColorBrushEffect(drawingContext.GraphicsDevice, bytecode);
    }

    public Vector4 Color
    {
        get => _color.GetValueVector4();
        set => _color.SetValue(value);
    }

    private void Initialize(EffectParameterCollection parameters, out EffectParameter color)
    {
        color = parameters["g_color"];
    }

    private readonly EffectParameter _color;

}
