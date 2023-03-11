using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.Drawing.Effects;

internal abstract class BrushEffect : Effect
{

    protected BrushEffect(GraphicsDevice graphicsDevice, byte[] effectCode)
        : base(graphicsDevice, effectCode)
    {
        Initialize(Parameters, out _opacity, out _worldViewProjection);
    }

    public abstract void Apply();

    public float Opacity
    {
        get => _opacity.GetValueSingle();
        set => _opacity.SetValue(MathHelper.Clamp(value, 0, 1));
    }

    public void SetWorldViewProjection(Matrix world, Matrix view, Matrix projection)
    {
        var wvp = world * view * projection;
        _worldViewProjection.SetValue(wvp);
    }

    public static readonly Matrix DefaultWorld = Matrix.Identity;

    public static readonly Matrix DefaultView = Matrix.CreateLookAt(Vector3.UnitZ, Vector3.Zero, Vector3.UnitY);

    private static void Initialize(EffectParameterCollection parameters, out EffectParameter opacity, out EffectParameter worldViewProjection)
    {
        opacity = parameters["g_opacity"];
        worldViewProjection = parameters["g_wvp"];
    }

    private readonly EffectParameter _opacity;
    private readonly EffectParameter _worldViewProjection;

}
