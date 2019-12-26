using System;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.VideoPlayback {
    internal struct RenderTargetSwitcher : IDisposable {

        private RenderTargetSwitcher([NotNull] GraphicsDevice graphicsDevice, [NotNull] RenderTarget2D renderTarget) {
            _graphicsDevice = graphicsDevice;
            _originalTargets = graphicsDevice.GetRenderTargets();
            graphicsDevice.SetRenderTarget(renderTarget);
        }

        public static RenderTargetSwitcher SwitchTo([NotNull] GraphicsDevice graphicsDevice, [NotNull] RenderTarget2D renderTarget) {
            return new RenderTargetSwitcher(graphicsDevice, renderTarget);
        }

        public void Dispose() {
            _graphicsDevice.SetRenderTargets(_originalTargets);
        }

        [NotNull]
        private readonly GraphicsDevice _graphicsDevice;

        [NotNull]
        private readonly RenderTargetBinding[] _originalTargets;

    }
}
