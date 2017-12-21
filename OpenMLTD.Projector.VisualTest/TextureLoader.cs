using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace OpenMLTD.Projector.VisualTest {
    internal static class TextureLoader {

        internal static Texture2D LoadTexture(GraphicsDevice graphicsDevice, string assetPath) {
            using (var fileStream = File.Open(assetPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                var texture = Texture2D.FromStream(graphicsDevice, fileStream);

                return texture;
            }
        }

    }
}
