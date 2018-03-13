using System;
using System.Windows.Forms;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.WinForms {
    public sealed class GraphicsDeviceService : DisposableBase, IGraphicsDeviceService {

        public GraphicsDeviceService([NotNull] IWin32Window window, GraphicsProfile profile) {
            CreateDevice(window.Handle, 1, 1, profile);
        }

        [NotNull]
        public GraphicsDevice GraphicsDevice => _device;

        public void ResetDevice(int width, int height) {
            DeviceResetting?.Invoke(this, EventArgs.Empty);
            DeviceReset?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<EventArgs> DeviceCreated;
        public event EventHandler<EventArgs> DeviceDisposing;
        public event EventHandler<EventArgs> DeviceReset;
        public event EventHandler<EventArgs> DeviceResetting;

        protected override void Dispose(bool disposing) {
            if (disposing) {
                DeviceDisposing?.Invoke(this, EventArgs.Empty);
                _device.Dispose();
            }
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

        private GraphicsDevice _device;

    }
}
