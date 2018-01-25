using System;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Drawing.Geometries;

namespace MonoGame.Extended.Drawing {
    public sealed class DrawingContext {

        public DrawingContext([NotNull] GraphicsDevice graphicsDevice) {
            GraphicsDevice = graphicsDevice;
        }

        public GraphicsDevice GraphicsDevice { get; }

        public static void DrawEllipse([NotNull] Pen pen, Ellipse ellipse) {
            var ellipseGeo = new EllipseGeometry(ellipse);

            DrawGeometry(pen, ellipseGeo);
        }

        public static void DrawGeometry([NotNull] Pen pen, [NotNull] Geometry geometry) {
            var mesh = new Mesh();
            var tessSink = mesh.Open();

            geometry.TessellateOutline(pen.StrokeWidth, pen.StrokeStyle, null, Geometry.DefaultFlatteningTolerance, tessSink);

            FillMesh(pen.Brush, mesh);
        }

        public static void DrawLine([NotNull] Pen pen, Vector2 point1, Vector2 point2) {
            var lineGeo = new LineGeometry(point1, point2);

            DrawGeometry(pen, lineGeo);
        }

        public static void DrawRectangle([NotNull] Pen pen, RectangleF rectangle) {
            var rectGeo = new RectangleGeometry(rectangle);

            DrawGeometry(pen, rectGeo);
        }

        public static void DrawRoundedRectangle([NotNull] Pen pen, RoundedRectangle roundedRectangle) {
            var roundedRectGeo = new RoundedRectangleGeometry(roundedRectangle);

            DrawGeometry(pen, roundedRectGeo);
        }

        public static void FillEllipse([NotNull] Brush brush, Ellipse ellipse) {
            var ellipseGeo = new EllipseGeometry(ellipse);

            FillGeometry(brush, ellipseGeo);
        }

        public static void FillGeometry([NotNull] Brush brush, [NotNull] Geometry geometry) {
            var mesh = new Mesh();
            var tessSink = mesh.Open();

            geometry.Tessellate(tessSink);

            FillMesh(brush, mesh);
        }

        public static void FillMesh([NotNull] Brush brush, [NotNull] Mesh mesh) {
            Guard.EnsureArgumentNotNull(brush, nameof(brush));

            if (mesh.Triangles == null) {
                return;
            }

            brush.Render(mesh.Triangles);
        }

        public static void FillRectangle([NotNull] Brush brush, RectangleF rectangle) {
            var rectGeo = new RectangleGeometry(rectangle);

            FillGeometry(brush, rectGeo);
        }

        public static void FillRoundedRectangle([NotNull] Brush brush, RoundedRectangle roundedRectangle) {
            var roundedRectGeo = new RoundedRectangleGeometry(roundedRectangle);

            FillGeometry(brush, roundedRectGeo);
        }

        public static GraphicsBackend GraphicsBackend {
            set {
                if (value == GraphicsBackend.Unknown) {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "You cannot set graphics backend to unknown.");
                }

                if (_graphicsBackend != GraphicsBackend.Unknown) {
                    throw new InvalidOperationException();
                }

                _graphicsBackend = value;
            }
            internal get => _graphicsBackend;
        }

        private static GraphicsBackend _graphicsBackend = GraphicsBackend.Unknown;

    }
}
