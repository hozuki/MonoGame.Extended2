using JetBrains.Annotations;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.Drawing {
    public sealed class Mesh : SinkOpener<TessellationSink> {

        [CanBeNull]
        internal Triangle[] Triangles => _triangles;

        protected override TessellationSink CreateSink() {
            return new TessellationSink(this);
        }

        protected override void OnSinkClosed(TessellationSink sink) {
            _triangles = sink.Triangles;

            base.OnSinkClosed(sink);
        }

        [CanBeNull]
        private Triangle[] _triangles;

    }
}
