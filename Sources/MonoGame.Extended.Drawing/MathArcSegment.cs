using Microsoft.Xna.Framework;
using MonoGame.Extended.Drawing.Geometries;

namespace MonoGame.Extended.Drawing {
    /// <summary>
    /// Only used for <see cref="EllipseGeometry"/>; don't use it for anything else!
    /// </summary>
    internal struct MathArcSegment {

        internal Vector2 Center { get; set; }

        internal Vector2 Radius { get; set; }

        internal float StartAngle { get; set; }

        internal float SweepAngle { get; set; }

        internal float RotationAngle { get; set; }

    }
}
