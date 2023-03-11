using System;
using System.Runtime.Versioning;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.WinForms.WindowsDX;

[SupportedOSPlatform("windows7.0")]
public sealed class SwapChainUpdatedEventArgs : EventArgs
{

    public SwapChainUpdatedEventArgs(SwapChainRenderTarget swapChainRenderTarget)
    {
        Guard.ArgumentNotNull(swapChainRenderTarget, nameof(swapChainRenderTarget));

        SwapChain = swapChainRenderTarget;
    }

    public SwapChainRenderTarget SwapChain { get; }

}
