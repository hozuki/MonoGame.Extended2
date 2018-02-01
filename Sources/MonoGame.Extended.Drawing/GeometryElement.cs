using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Drawing.Geometries;

namespace MonoGame.Extended.Drawing {
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    internal struct GeometryElement {

        internal GeometryElement(Vector2 lineSegment)
            : this() {
            _type = GeometryElementType.Line;
            _lineSegment = lineSegment;
        }

        internal GeometryElement(ArcSegment arcSegment)
            : this() {
            _type = GeometryElementType.Arc;
            _arcSegment = arcSegment;
        }

        internal GeometryElement(BezierSegment bezierSegment)
            : this() {
            _type = GeometryElementType.Bezier;
            _bezierSegment = bezierSegment;
        }

        internal GeometryElement(QuadraticBezierSegment quadraticBezierSegment)
            : this() {
            _type = GeometryElementType.QuadraticBezier;
            _quadraticBezierSegment = quadraticBezierSegment;
        }

        /// <summary>
        /// Only used for <see cref="EllipseGeometry"/>; don't use it for anything else!
        /// </summary>
        internal GeometryElement(MathArcSegment mathArcSegment)
            : this() {
            _type = GeometryElementType.MathArc;
            _mathArcSegment = mathArcSegment;
        }

        internal GeometryElementType Type {
            get => _type;
            set => _type = value;
        }

        internal Vector2 LineSegment {
            get => _lineSegment;
            set => _lineSegment = value;
        }

        internal ArcSegment ArcSegment {
            get => _arcSegment;
            set => _arcSegment = value;
        }

        internal BezierSegment BezierSegment {
            get => _bezierSegment;
            set => _bezierSegment = value;
        }

        internal QuadraticBezierSegment QuadraticBezierSegment {
            get => _quadraticBezierSegment;
            set => _quadraticBezierSegment = value;
        }

        /// <summary>
        /// Only used for <see cref="EllipseGeometry"/>; don't use it for anything else!
        /// </summary>
        internal MathArcSegment MathArcSegment {
            get => _mathArcSegment;
            set => _mathArcSegment = value;
        }

        [FieldOffset(0)]
        private GeometryElementType _type;

        [FieldOffset(4)]
        private Vector2 _lineSegment;
        [FieldOffset(4)]
        private ArcSegment _arcSegment;
        [FieldOffset(4)]
        private BezierSegment _bezierSegment;
        [FieldOffset(4)]
        private QuadraticBezierSegment _quadraticBezierSegment;
        [FieldOffset(4)]
        private MathArcSegment _mathArcSegment;

    }
}
