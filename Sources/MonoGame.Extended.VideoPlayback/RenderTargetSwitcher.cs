using System;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.VideoPlayback;

internal struct RenderTargetSwitcher : IDisposable
{

    private RenderTargetSwitcher(GraphicsDevice graphicsDevice, RenderTarget2D renderTarget)
    {
        _graphicsDevice = graphicsDevice;
        _originalTargets = graphicsDevice.GetRenderTargets();
        graphicsDevice.SetRenderTarget(renderTarget);
    }

    public static RenderTargetSwitcher SwitchTo(GraphicsDevice graphicsDevice, RenderTarget2D renderTarget)
    {
        return new RenderTargetSwitcher(graphicsDevice, renderTarget);
    }

    public void Dispose()
    {
        _graphicsDevice.SetRenderTargets(_originalTargets);
    }

    private readonly GraphicsDevice _graphicsDevice;

    private readonly RenderTargetBinding[] _originalTargets;

}
