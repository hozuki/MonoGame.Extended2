using Microsoft.Xna.Framework;
using MonoGame.Extended.Drawing.Geometries;

namespace MonoGame.Extended.Drawing;

/// <summary>
/// Only used for <see cref="EllipseGeometry"/>; don't use it for anything else!
/// </summary>
internal struct MathArcSegment
{

    public Vector2 Center { get; set; }

    public Vector2 Radius { get; set; }

    public float StartAngle { get; set; }

    public float SweepAngle { get; set; }

    public float RotationAngle { get; set; }

}
