using System;
using Microsoft.Xna.Framework;

namespace MonoGame.Extended.Drawing.Geometries {
    public sealed class RoundedRectangleGeometry : Geometry {

        public RoundedRectangleGeometry(RoundedRectangle roundedRectangle) {
            _roundedRectangle = roundedRectangle;
            InitializeShape();
        }

        public override GeometrySink Open() {
            throw new NotSupportedException();
        }

        private void InitializeShape() {
            // TODO: We did't verify the shape.
            var radius = new Vector2(_roundedRectangle.RadiusX, _roundedRectangle.RadiusY);
            var rect = _roundedRectangle.Rectangle;

            var sink = base.Open();

            sink.BeginFigure(new Vector2(rect.Right, (rect.Top + rect.Bottom) / 2));
            sink.AddLine(new Vector2(rect.Right, rect.Bottom - radius.Y));
            sink.AddArc(new ArcSegment {
                ArcSize = ArcSize.Small,
                Point = new Vector2(rect.Right - radius.X, rect.Bottom),
                RotationAngle = 0,
                Size = radius,
                SweepDirection = SweepDirection.Clockwise
            });
            sink.AddLine(new Vector2(rect.Left + radius.X, rect.Bottom));
            sink.AddArc(new ArcSegment {
                ArcSize = ArcSize.Small,
                Point = new Vector2(rect.Left, rect.Bottom - radius.Y),
                RotationAngle = 0,
                Size = radius,
                SweepDirection = SweepDirection.Clockwise
            });
            sink.AddLine(new Vector2(rect.Left, rect.Top + radius.Y));
            sink.AddArc(new ArcSegment {
                ArcSize = ArcSize.Small,
                Point = new Vector2(rect.Left + radius.X, rect.Top),
                RotationAngle = 0,
                Size = radius,
                SweepDirection = SweepDirection.Clockwise
            });
            sink.AddLine(new Vector2(rect.Right - radius.X, rect.Top));
            sink.AddArc(new ArcSegment {
                ArcSize = ArcSize.Small,
                Point = new Vector2(rect.Right, rect.Top + radius.Y),
                RotationAngle = 0,
                Size = radius,
                SweepDirection = SweepDirection.Clockwise
            });
            sink.EndFigure(FigureEnd.Closed); // ... so we don't need to manually add the final line (to the origin).

            sink.Close();
        }

        private readonly RoundedRectangle _roundedRectangle;

    }
}
