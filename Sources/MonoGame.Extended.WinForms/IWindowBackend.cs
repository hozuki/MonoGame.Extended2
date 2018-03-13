using System;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.WinForms {
    public interface IWindowBackend : IDisposable {

        void Initialize([NotNull] GraphicsDeviceControl control);

        void PrepareDraw([NotNull] GraphicsDeviceControl control, [NotNull] PresentationParameters presentationParameters);

        void BeginDraw([NotNull] GraphicsDeviceControl control);

        void EndDraw([NotNull] GraphicsDeviceControl control);

        void OnWindowSizeChanged([NotNull] GraphicsDeviceControl control);

    }
}
