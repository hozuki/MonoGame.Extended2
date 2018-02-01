using MonoGame.Extended.Drawing.Geometries;

namespace MonoGame.Extended.Drawing {
    internal enum GeometryElementType {

        Line,
        Arc,
        Bezier,
        QuadraticBezier,
        /// <summary>
        /// Only used for <see cref="EllipseGeometry"/>; don't use it for anything else!
        /// </summary>
        MathArc

    }
}
