using System;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.WinForms {
    public sealed class GraphicsDeviceWrapper : IGraphicsDeviceService {

        private GraphicsDeviceWrapper() {
        }

        public GraphicsDevice GraphicsDevice => _device;

        public void ResetDevice(int width, int height) {
            DeviceResetting?.Invoke(this, EventArgs.Empty);
            DeviceReset?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<EventArgs> DeviceCreated;
        public event EventHandler<EventArgs> DeviceDisposing;
        public event EventHandler<EventArgs> DeviceReset;
        public event EventHandler<EventArgs> DeviceResetting;

        internal static GraphicsDeviceWrapper AddRef(IntPtr hWnd, int width, int height, GraphicsProfile profile) {
            if (Interlocked.Increment(ref _refCount) == 1) {
                Instance.CreateDevice(hWnd, width, height, profile);
            }

            return Instance;
        }

        internal void Release() {
            Release(true);
        }

        internal void Release(bool disposing) {
            if (Interlocked.Decrement(ref _refCount) != 0) {
                return;
            }

            if (disposing) {
                DeviceDisposing?.Invoke(this, EventArgs.Empty);
                _device.Dispose();
            }

            _device = null;
        }

        private void CreateDevice(IntPtr hWnd, int width, int height, GraphicsProfile profile) {
            var adapter = GraphicsAdapter.DefaultAdapter;
            var pp = new PresentationParameters {
                DeviceWindowHandle = hWnd,
                BackBufferWidth = Math.Max(width, 1),
                BackBufferHeight = Math.Max(height, 1),
                BackBufferFormat = SurfaceFormat.Color,
                DepthStencilFormat = DepthFormat.Depth16
            };

            _device = new GraphicsDevice(adapter, profile, pp);

            DeviceCreated?.Invoke(this, EventArgs.Empty);
        }

        private static readonly GraphicsDeviceWrapper Instance = new GraphicsDeviceWrapper();
        private static int _refCount;

        private GraphicsDevice _device;

    }
}
