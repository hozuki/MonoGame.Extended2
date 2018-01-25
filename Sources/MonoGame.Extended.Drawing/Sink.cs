using System;

namespace MonoGame.Extended.Drawing {
    public abstract class Sink : ISink {

        public void Close() {
            EnsureNotClosed();

            _isClosed = true;

            OnClosed();
        }

        protected virtual void OnClosed() {
        }

        protected void EnsureNotClosed() {
            if (_isClosed) {
                throw new InvalidOperationException();
            }
        }

        private bool _isClosed;

    }
}
