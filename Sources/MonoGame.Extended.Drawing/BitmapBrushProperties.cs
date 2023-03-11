using JetBrains.Annotations;

namespace MonoGame.Extended.Drawing;

[PublicAPI]
public struct BitmapBrushProperties
{

    public ExtendMode ExtendModeX { get; set; }

    public ExtendMode ExtendModeY { get; set; }

    public BitmapInterpolationMode InterpolationMode { get; set; }

}
