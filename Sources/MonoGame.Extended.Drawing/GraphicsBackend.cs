using JetBrains.Annotations;

namespace MonoGame.Extended.Drawing;

[PublicAPI]
public enum GraphicsBackend
{

    Unknown = 0,
    Direct3D11 = 1,
    OpenGL = 2,
    PlayStation4 = 3,

}
