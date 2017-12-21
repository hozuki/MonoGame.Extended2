using System;
using MonoGame.Extended.DesktopGL.VideoPlayback;

namespace Demo.DesktopGL.VideoPlayback {
    /// <summary>
    /// The main class.
    /// </summary>
    internal static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main() {
            FFmpegBinariesHelper.InitializeFFmpeg("x86", "x64");

            using (var game = new Game1()) {
                game.Run();
            }
        }
    }
}
