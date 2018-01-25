using System;
using JetBrains.Annotations;

namespace MonoGame.Extended.Drawing {
    public abstract class SinkOpener<TSink> where TSink : class, ISink {

        public virtual TSink Open() {
            if (_activeSink != null) {
                throw new InvalidOperationException();
            }

            var sink = CreateSink();

            _activeSink = sink;

            return sink;
        }

        internal void CloseSink([NotNull] TSink sink) {
            if (_activeSink != sink) {
                throw new ArgumentException("Sink not match.", nameof(sink));
            }

            OnSinkClosed(sink);

            _activeSink = null;
        }

        protected abstract TSink CreateSink();

        protected virtual void OnSinkClosed([NotNull] TSink sink) {
        }

        private TSink _activeSink;

    }
}
