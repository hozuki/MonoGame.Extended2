using System.Collections.Generic;
using JetBrains.Annotations;

namespace MonoGame.Extended.Drawing {
    public sealed class TessellationSink : Sink {

        internal TessellationSink([NotNull] Mesh mesh) {
            _mesh = mesh;
            _triangles = new List<Triangle>();
        }

        public void AddTriangle(Triangle triangle) {
            EnsureNotClosed();

            _triangles.Add(triangle);
        }

        public void AddTriangles([NotNull] Triangle[] triangles) {
            EnsureNotClosed();

            _triangles.AddRange(triangles);
        }

        internal Triangle[] Triangles { get; private set; }

        protected override void OnClosed() {
            Triangles = _triangles.ToArray();
            _mesh.CloseSink(this);

            base.OnClosed();
        }

        private readonly List<Triangle> _triangles;

        private readonly Mesh _mesh;

    }
}
