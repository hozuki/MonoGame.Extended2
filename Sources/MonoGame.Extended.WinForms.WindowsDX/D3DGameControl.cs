using System.Runtime.Versioning;

namespace MonoGame.Extended.WinForms.WindowsDX;

[SupportedOSPlatform("windows7.0")]
public class D3DGameControl : GameControl
{

    public D3DGameControl()
    {
        WindowBackend = new D3DWindowBackend();
    }

}
