using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Graphics;
using SharpFont;

namespace MonoGame.Extended.Text.Extensions {
    /// <summary>
    /// Extension methods for <see cref="FTBitmap"/>.
    /// </summary>
    internal static class FTBitmapExtensions {

        /// <summary>
        /// Renders a <see cref="FTBitmap"/> to a <see cref="Texture2D"/>.
        /// </summary>
        /// <remarks>
        /// Size of the texture must be equal to or larger than size of the bitmap.
        /// The format of the bitmap must be <see cref="PixelMode.Gray"/>, and the format of the texture must be a 32-bit format
        /// (<see cref="SurfaceFormat.Bgr32"/>, <see cref="SurfaceFormat.Bgra32"/>, or <see cref="SurfaceFormat.Color"/>).
        /// </remarks>
        /// <param name="bitmap">The <see cref="FTBitmap"/> containing character image.</param>
        /// <param name="texture">The <see cref="Texture2D"/> to render to.</param>
        public static void RenderToTexture([NotNull] this FTBitmap bitmap, [NotNull] Texture2D texture) {
            Debug.Assert(texture.Width >= bitmap.Width);
            Debug.Assert(texture.Height >= bitmap.Rows);
            Debug.Assert(bitmap.PixelMode == PixelMode.Gray);

            var textureFormat = texture.Format;

            Debug.Assert(textureFormat == SurfaceFormat.Bgr32 | textureFormat == SurfaceFormat.Bgra32 || textureFormat == SurfaceFormat.Color);

            // Are pixel bytes in A-R-G-B order?
            var argb = textureFormat == SurfaceFormat.Color;

            var textureData = new uint[texture.Width * texture.Height];
            var bitmapData = bitmap.BufferData;

            for (var j = 0; j < bitmap.Rows; j++) {
                var bitmapLineStart = j * bitmap.Pitch;
                var textureLineStart = j * texture.Width;

                for (var i = 0; i < bitmap.Width; i++) {
                    var bitmapPixelIndex = bitmapLineStart + i;
                    var texturePixelIndex = textureLineStart + i;

                    var alpha = (uint)bitmapData[bitmapPixelIndex];
                    var color = alpha;

                    if (argb) {
                        textureData[texturePixelIndex] = color << 16 | color << 8 | color | alpha << 24;
                    } else {
                        textureData[texturePixelIndex] = color << 24 | color << 16 | color << 8 | alpha;
                    }
                }
            }

            texture.SetData(textureData);
        }

    }
}
