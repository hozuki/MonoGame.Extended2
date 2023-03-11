using System;
using System.Diagnostics;
using System.Runtime.Versioning;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.WinForms.WindowsDX;

[SupportedOSPlatform("windows7.0")]
public sealed class D3DWindowBackend : DisposableBase, IWindowBackend
{

    internal D3DWindowBackend()
    {
    }

    /// <summary>
    /// Transfers the new <see cref="SwapChainRenderTarget"/> to the editor service object after resizing a custom control.
    /// Make sure to subscribe to this event in your custom control to update your custom <see cref="RenderTarget2D"/>s
    /// according to the recent changes in the back buffer (width and height of back buffers).
    /// </summary>
    public event EventHandler<SwapChainUpdatedEventArgs>? SwapChainUpdated;

    public SwapChainRenderTarget? SwapChain => _chain;

    public void Initialize(GraphicsDeviceControl control, PresentInterval presentInterval)
    {
        if (control.IsDesignMode)
        {
            return;
        }

        var clientSize = control.ClientSize;

        var width = Math.Max(clientSize.Width, 1);
        var height = Math.Max(clientSize.Height, 1);

        var mipMap = true;
        var surfaceFormat = SurfaceFormat.Color;
        var depthFormat = DepthFormat.Depth24Stencil8;
        var preferredMultiSampleCount = 1;
        var renderTargetUsage = RenderTargetUsage.PreserveContents;

        _chain = new SwapChainRenderTarget(control.GraphicsDevice, control.Handle, width, height, mipMap, surfaceFormat, depthFormat, preferredMultiSampleCount, renderTargetUsage, presentInterval);
    }

    public void PrepareDraw(GraphicsDeviceControl control)
    {
    }

    public void BeginDraw(GraphicsDeviceControl control)
    {
        if (control.IsDesignMode)
        {
            return;
        }

        var graphicsDevice = control.GraphicsDevice;

        graphicsDevice?.SetRenderTarget(_chain);
    }

    public void EndDraw(GraphicsDeviceControl control)
    {
        if (control.IsDesignMode)
        {
            return;
        }

        if (_chain is null)
        {
            throw new InvalidOperationException("A swap chain is not created for this backend.");
        }

        try
        {
            _chain.Present();
        }
        catch (Exception ex)
        {
            Debug.Print(ex.ToString());
        }
    }

    public void OnWindowSizeChanged(GraphicsDeviceControl control)
    {
        var clientSize = control.ClientSize;

        if (clientSize.Width <= 0 || clientSize.Height <= 0)
        {
            return;
        }

        _chain?.Dispose();

        var graphicsDevice = control.GraphicsDevice;

        if (graphicsDevice is not null)
        {
            _chain = new SwapChainRenderTarget(graphicsDevice, control.Handle, clientSize.Width, clientSize.Height);

            graphicsDevice.PresentationParameters.BackBufferWidth = clientSize.Width;
            graphicsDevice.PresentationParameters.BackBufferHeight = clientSize.Height;

            graphicsDevice.Viewport = new Viewport(0, 0, clientSize.Width, clientSize.Height, 0, 1);
        }

        if (_chain is not null)
        {
            SwapChainUpdated?.Invoke(this, new SwapChainUpdatedEventArgs(_chain));
        }
    }

    protected override void Dispose(bool disposing)
    {
        _chain?.Dispose();
        _chain = null;
    }

    private SwapChainRenderTarget? _chain;

}
