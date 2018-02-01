using System;
using Microsoft.Xna.Framework;

namespace MonoGame.Extended.Drawing.Geometries {
    public sealed class RectangleGeometry : Geometry {

        public RectangleGeometry(RectangleF rectangle) {
            _rectangle = rectangle;
            InitializeShape();
        }

        public override GeometrySink Open() {
            throw new NotSupportedException();
        }

        private void InitializeShape() {
            var rect = _rectangle;

            var sink = base.Open();

            sink.BeginFigure(new Vector2(rect.Left, rect.Top));
            sink.AddLine(new Vector2(rect.Right, rect.Top));
            sink.AddLine(new Vector2(rect.Right, rect.Bottom));
            sink.AddLine(new Vector2(rect.Left, rect.Bottom));
            sink.EndFigure(FigureEnd.Closed);

            sink.Close();
        }

        private readonly RectangleF _rectangle;

    }
}
