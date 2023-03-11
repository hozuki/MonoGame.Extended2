using System.IO;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Graphics;

namespace Demo {
    public static class TextureLoader {

        public static Texture2D LoadTexture(GraphicsDevice graphicsDevice, string assetPath) {
            using (var fileStream = File.Open(assetPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                var texture = Texture2D.FromStream(graphicsDevice, fileStream);

                return texture;
            }
        }

    }
}
