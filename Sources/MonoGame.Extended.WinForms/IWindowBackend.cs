using System;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.WinForms;

public interface IWindowBackend : IDisposable
{

    void Initialize(GraphicsDeviceControl control, PresentInterval presentInterval);

    void PrepareDraw(GraphicsDeviceControl control);

    void BeginDraw(GraphicsDeviceControl control);

    void EndDraw(GraphicsDeviceControl control);

    void OnWindowSizeChanged(GraphicsDeviceControl control);

}
