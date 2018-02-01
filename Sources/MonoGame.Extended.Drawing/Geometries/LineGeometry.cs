using System;
using Microsoft.Xna.Framework;

namespace MonoGame.Extended.Drawing.Geometries {
    internal sealed class LineGeometry : Geometry {

        public LineGeometry(Vector2 point1, Vector2 point2) {
            _point1 = point1;
            _point2 = point2;
            InitializeShape();
        }

        public override GeometrySink Open() {
            throw new NotSupportedException();
        }

        private void InitializeShape() {
            var sink = base.Open();

            sink.BeginFigure(_point1);
            sink.AddLine(_point2);
            sink.EndFigure(FigureEnd.Open);

            sink.Close();
        }

        private readonly Vector2 _point1;
        private readonly Vector2 _point2;

    }
}
