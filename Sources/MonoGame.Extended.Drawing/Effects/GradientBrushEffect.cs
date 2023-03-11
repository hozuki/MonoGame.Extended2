using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.Drawing.Effects;

internal abstract class GradientBrushEffect : BrushEffect
{

    protected GradientBrushEffect(GraphicsDevice graphicsDevice, byte[] effectCode)
        : base(graphicsDevice, effectCode)
    {
        Initialize(Parameters, out _gradientStopColors, out _gradientStopPositions, out _numGradientStops);
    }

    public void SetGradientStops(GradientStop[] gradientStops)
    {
        var colors = new Vector4[GradientStopCollection.MaximumGradientStops];
        var positions = new float[GradientStopCollection.MaximumGradientStops];

        for (var i = 0; i < gradientStops.Length; ++i)
        {
            var gs = gradientStops[i];
            colors[i] = gs.Color.ToVector4();
            positions[i] = gs.Position;
        }

        _gradientStopColors.SetValue(colors);
        _gradientStopPositions.SetValue(positions);
        _numGradientStops.SetValue(gradientStops.Length);
    }

    public Gamma Gamma { get; set; }

    public ExtendMode ExtendMode { get; set; }

    private static void Initialize(EffectParameterCollection parameters, out EffectParameter gradientStopColors, out EffectParameter gradientStopPositions, out EffectParameter numGradientStops)
    {
        gradientStopColors = parameters["g_gradientStopColors"];
        gradientStopPositions = parameters["g_gradientStopPositions"];
        numGradientStops = parameters["g_numGradientStops"];
    }

    private readonly EffectParameter _gradientStopColors;
    private readonly EffectParameter _gradientStopPositions;
    private readonly EffectParameter _numGradientStops;

}
