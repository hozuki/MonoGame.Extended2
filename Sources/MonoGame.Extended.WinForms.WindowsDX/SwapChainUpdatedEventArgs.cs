using System;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.WinForms.WindowsDX {
    public sealed class SwapChainUpdatedEventArgs : EventArgs {

        public SwapChainUpdatedEventArgs([NotNull] SwapChainRenderTarget swapChainRenderTarget) {
            Guard.ArgumentNotNull(swapChainRenderTarget, nameof(swapChainRenderTarget));

            SwapChain = swapChainRenderTarget;
        }

        [NotNull]
        public SwapChainRenderTarget SwapChain { get; }

    }
}
