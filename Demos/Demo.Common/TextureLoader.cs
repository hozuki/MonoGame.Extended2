using System.IO;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Graphics;

namespace Demo {
    public static class TextureLoader {

        [NotNull]
        public static Texture2D LoadTexture([NotNull] GraphicsDevice graphicsDevice, [NotNull] string assetPath) {
            using (var fileStream = File.Open(assetPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                var texture = Texture2D.FromStream(graphicsDevice, fileStream);

                return texture;
            }
        }

    }
}
