using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Drawing.Geometries;

namespace MonoGame.Extended.Drawing {
    public sealed class DrawingContext : DisposableBase {

        public DrawingContext([NotNull] GraphicsDevice graphicsDevice, GraphicsBackend backend) {
            GraphicsDevice = graphicsDevice;
            Backend = backend;
            EffectResources = new DrawingContextEffectResources(this);

            graphicsDevice.DeviceReset += GraphicsDevice_DeviceReset;
            UpdateProjectionMatrix();
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

            brush.Render(mesh.Triangles, _currentTransform);
        }

        public void FillRectangle([NotNull] Brush brush, RectangleF rectangle) {
            var rectGeo = new RectangleGeometry(rectangle);

            FillGeometry(brush, rectGeo);
        }

        public void FillRoundedRectangle([NotNull] Brush brush, RoundedRectangle roundedRectangle) {
            var roundedRectGeo = new RoundedRectangleGeometry(roundedRectangle);

            FillGeometry(brush, roundedRectGeo);
        }

        #region Transforms
        public void SetCurrentTransform(Matrix3x2 transform) {
            _currentTransform = transform;
        }

        public void Translate(float x, float y) {
            _currentTransform *= Matrix3x2.CreateTranslation(x, y);
        }

        public void Translate(Vector2 translation) {
            _currentTransform *= Matrix3x2.CreateTranslation(translation);
        }

        public void PushTransform() {
            _transforms.Push(_currentTransform);
        }

        public Matrix3x2 PopTransform() {
            if (_transforms.Count == 0) {
                throw new InvalidOperationException("No pushed transforms in stack.");
            }

            var popped = _transforms.Pop();
            _currentTransform = popped;

            return popped;
        }
        #endregion

        internal DrawingContextEffectResources EffectResources { get; }

        /// <summary>
        /// This orthographic projection matrix has a NEGATIVE Y position (i.e. y to 0), so it
        /// helps drawing elements to move to the fourth quadrant.
        /// </summary>
        internal Matrix DefaultOrthographicProjection { get; private set; }

        protected override void Dispose(bool disposing) {
            GraphicsDevice.DeviceReset -= GraphicsDevice_DeviceReset;
        }

        private void UpdateProjectionMatrix() {
            var viewport = GraphicsDevice.Viewport;

            DefaultOrthographicProjection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0.5f, 10f);
        }

        private void GraphicsDevice_DeviceReset(object sender, EventArgs e) {
            UpdateProjectionMatrix();
        }

        private Matrix3x2 _currentTransform = Matrix3x2.Identity;
        private readonly Stack<Matrix3x2> _transforms = new Stack<Matrix3x2>();

    }
}
