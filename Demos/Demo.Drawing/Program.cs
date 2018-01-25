using System;
using MonoGame.Extended.Drawing;

namespace Demo.Drawing {
    /// <summary>
    /// The main class.
    /// </summary>
    internal static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main() {
            DrawingContext.GraphicsBackend = GraphicsBackend.OpenGL;

            using (var game = new Game1()) {
                game.Run();
            }
        }
    }
}
