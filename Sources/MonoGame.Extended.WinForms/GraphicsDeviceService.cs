using System;
using System.Windows.Forms;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.WinForms;

public sealed class GraphicsDeviceService : DisposableBase, IGraphicsDeviceService
{

    public GraphicsDeviceService(IWin32Window window, GraphicsProfile profile)
    {
        _device = CreateDevice(window.Handle, 1, 1, profile);
        DeviceCreated?.Invoke(this, EventArgs.Empty);
    }

    public GraphicsDevice GraphicsDevice => _device;

    public void ResetDevice(int width, int height)
    {
        DeviceResetting?.Invoke(this, EventArgs.Empty);
        DeviceReset?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler<EventArgs>? DeviceCreated;
    public event EventHandler<EventArgs>? DeviceDisposing;
    public event EventHandler<EventArgs>? DeviceReset;
    public event EventHandler<EventArgs>? DeviceResetting;

    protected override void Dispose(bool disposing)
    {
        DeviceDisposing?.Invoke(this, EventArgs.Empty);

        if (disposing)
        {
            _device.Dispose();
        }
    }

    private static GraphicsDevice CreateDevice(IntPtr hWnd, int width, int height, GraphicsProfile profile)
    {
        var adapter = GraphicsAdapter.DefaultAdapter;
        var pp = new PresentationParameters
        {
            DeviceWindowHandle = hWnd,
            BackBufferWidth = Math.Max(width, 1),
            BackBufferHeight = Math.Max(height, 1),
            BackBufferFormat = SurfaceFormat.Color,
            DepthStencilFormat = DepthFormat.Depth24Stencil8
        };

        var device = new GraphicsDevice(adapter, profile, pp);
        return device;
    }

    private readonly GraphicsDevice _device;

}
