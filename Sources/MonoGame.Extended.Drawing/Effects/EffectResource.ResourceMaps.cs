using System.Collections.Generic;

namespace MonoGame.Extended.Drawing.Effects;

partial class EffectResource
{

    private static readonly IReadOnlyDictionary<GraphicsBackend, string> SolidColorBrushResourceNames = new Dictionary<GraphicsBackend, string>
    {
        [GraphicsBackend.Direct3D11] = SolidColorBrushEffect_Direct3D11,
        [GraphicsBackend.OpenGL] = SolidColorBrushEffect_OpenGL,
    };

    private static readonly IReadOnlyDictionary<GraphicsBackend, string> BitmapBrushResourceNames = new Dictionary<GraphicsBackend, string>
    {
        [GraphicsBackend.Direct3D11] = BitmapBrushEffect_Direct3D11,
        [GraphicsBackend.OpenGL] = BitmapBrushEffect_OpenGL,
    };

    private static readonly IReadOnlyDictionary<GraphicsBackend, string> LinearGradientBrushResourceNames = new Dictionary<GraphicsBackend, string>
    {
        [GraphicsBackend.Direct3D11] = LinearGradientBrushEffect_Direct3D11,
        [GraphicsBackend.OpenGL] = LinearGradientBrushEffect_OpenGL,
    };

}
