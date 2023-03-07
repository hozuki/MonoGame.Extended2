using System;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.WinForms {
    public interface IWindowBackend : IDisposable {

        void Initialize([NotNull] GraphicsDeviceControl control, PresentInterval presentInterval);

        void PrepareDraw([NotNull] GraphicsDeviceControl control);

        void BeginDraw([NotNull] GraphicsDeviceControl control);

        void EndDraw([NotNull] GraphicsDeviceControl control);

        void OnWindowSizeChanged([NotNull] GraphicsDeviceControl control);

    }
}
