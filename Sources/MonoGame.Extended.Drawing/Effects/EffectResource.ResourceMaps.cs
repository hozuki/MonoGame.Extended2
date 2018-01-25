using System.Collections.Generic;

namespace MonoGame.Extended.Drawing.Effects {
    partial class EffectResource {

        private static readonly IReadOnlyDictionary<GraphicsBackend, string> SolidBrushResourceNames = new Dictionary<GraphicsBackend, string>(2) {
            [GraphicsBackend.Direct3D11] = SolidBrushEffect_Direct3D11,
            [GraphicsBackend.OpenGL] = SolidBrushEffect_OpenGL
        };

    }
}
