using JetBrains.Annotations;
using Microsoft.Xna.Framework;

namespace MonoGame.Extended.Drawing;

[PublicAPI]
public struct Triangle
{

    public Vector2 Point1 { get; set; }

    public Vector2 Point2 { get; set; }

    public Vector2 Point3 { get; set; }

    public override string ToString()
    {
        return $"{Point1} {Point2} {Point3}";
    }

}
