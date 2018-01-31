using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Drawing.Geometries;

namespace MonoGame.Extended.Drawing {
    public sealed class DrawingContext {

        public DrawingContext([NotNull] GraphicsDevice graphicsDevice, GraphicsBackend backend) {
            GraphicsDevice = graphicsDevice;
            Backend = backend;
            EffectResources = new DrawingContextEffectResources(this);
        }

        public GraphicsDevice GraphicsDevice { get; }

        public GraphicsBackend Backend { get; }

        public void DrawEllipse([NotNull] Pen pen, Ellipse ellipse) {
            var ellipseGeo = new EllipseGeometry(ellipse);

            DrawGeometry(pen, ellipseGeo);
        }

        public void DrawGeometry([NotNull] Pen pen, [NotNull] Geometry geometry) {
            var mesh = new Mesh();
            var tessSink = mesh.Open();

            geometry.TessellateOutline(pen.StrokeWidth, pen.StrokeStyle, null, Geometry.DefaultFlatteningTolerance, tessSink);

            FillMesh(pen.Brush, mesh);
        }

        public void DrawLine([NotNull] Pen pen, Vector2 point1, Vector2 point2) {
            var lineGeo = new LineGeometry(point1, point2);

            DrawGeometry(pen, lineGeo);
        }

        public void DrawRectangle([NotNull] Pen pen, RectangleF rectangle) {
            var rectGeo = new RectangleGeometry(rectangle);

            DrawGeometry(pen, rectGeo);
        }

        public void DrawRoundedRectangle([NotNull] Pen pen, RoundedRectangle roundedRectangle) {
            var roundedRectGeo = new RoundedRectangleGeometry(roundedRectangle);

            DrawGeometry(pen, roundedRectGeo);
        }

        public void FillEllipse([NotNull] Brush brush, Ellipse ellipse) {
            var ellipseGeo = new EllipseGeometry(ellipse);

            FillGeometry(brush, ellipseGeo);
        }

        public void FillGeometry([NotNull] Brush brush, [NotNull] Geometry geometry) {
            var mesh = new Mesh();
            var tessSink = mesh.Open();

            geometry.Tessellate(tessSink);

            FillMesh(brush, mesh);
        }

        public void FillMesh([NotNull] Brush brush, [NotNull] Mesh mesh) {
            Guard.EnsureArgumentNotNull(brush, nameof(brush));

            if (mesh.Triangles == null) {
                return;
            }

            brush.Render(mesh.Triangles);
        }

        public void FillRectangle([NotNull] Brush brush, RectangleF rectangle) {
            var rectGeo = new RectangleGeometry(rectangle);

            FillGeometry(brush, rectGeo);
        }

        public void FillRoundedRectangle([NotNull] Brush brush, RoundedRectangle roundedRectangle) {
            var roundedRectGeo = new RoundedRectangleGeometry(roundedRectangle);

            FillGeometry(brush, roundedRectGeo);
        }

        internal DrawingContextEffectResources EffectResources { get; }

    }
}
