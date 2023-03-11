using JetBrains.Annotations;
using Microsoft.Xna.Framework;

namespace MonoGame.Extended.Drawing;

[PublicAPI]
public struct LinearGradientBrushProperties
{

    public Vector2 StartPoint { get; set; }

    public Vector2 EndPoint { get; set; }

}
