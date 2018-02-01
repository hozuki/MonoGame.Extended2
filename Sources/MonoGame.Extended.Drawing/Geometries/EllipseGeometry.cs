using System;
using Microsoft.Xna.Framework;

namespace MonoGame.Extended.Drawing.Geometries {
    public sealed class EllipseGeometry : Geometry {

        public EllipseGeometry(Ellipse ellipse) {
            _ellipse = ellipse;
            InitializeShape();
        }

        public override GeometrySink Open() {
            throw new NotSupportedException();
        }

        private void InitializeShape() {
            var ellipse = _ellipse;
            var sink = base.Open();

            var pt = ellipse.Point + new Vector2(ellipse.RadiusX, 0);

            sink.BeginFigure(pt);
            sink.AddMathArc(new MathArcSegment {
                Center = ellipse.Point,
                Radius = new Vector2(ellipse.RadiusX, ellipse.RadiusY),
                RotationAngle = 0,
                StartAngle = 0,
                SweepAngle = 360
            });
            sink.EndFigure(FigureEnd.Closed);

            sink.Close();
        }

        private readonly Ellipse _ellipse;

    }
}
