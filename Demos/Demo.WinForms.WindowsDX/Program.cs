using System;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace Demo.WinForms.WindowsDX;

[SupportedOSPlatform("windows7.0")]
internal static class Program
{

    /// <summary>
    /// 应用程序的主入口点。
    /// </summary>
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new Form1());
    }

}
