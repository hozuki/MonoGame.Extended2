using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.Text.Extensions {
    /// <summary>
    /// Extension methods for <see cref="SpriteBatch"/>.
    /// </summary>
    public static class SpriteBatchExtensions {

        /// <summary>
        /// Draws a string with variable line heights, using default scale and unconstrained bounds.
        /// </summary>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to use.</param>
        /// <param name="spriteFont">The <see cref="DynamicSpriteFont"/> to use.</param>
        /// <param name="str">The text to draw.</param>
        /// <param name="location">The top left location of drawn string.</param>
        /// <param name="spacing">The spacing factor. Its X component is used for character spacing, and Y component for line spacing.</param>
        /// <param name="color">The color of the text.</param>
        /// <returns>Size of the drawn string, in pixels.</returns>
        public static Vector2 DrawString([NotNull] this SpriteBatch spriteBatch, [NotNull] DynamicSpriteFont spriteFont, [CanBeNull] string str, Vector2 location, Vector2 spacing, Color color) {
            return DrawString(spriteBatch, spriteFont, str, location, Vector2.Zero, Vector2.One, spacing, color);
        }

        /// <summary>
        /// Draws a string with variable line heights, using default scale.
        /// </summary>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to use.</param>
        /// <param name="spriteFont">The <see cref="DynamicSpriteFont"/> to use.</param>
        /// <param name="str">The text to draw.</param>
        /// <param name="location">The top left location of drawn string.</param>
        /// <param name="maxBounds">Maximum bounds. Set its X and/or Y to 0 to disable the constraint on that dimension.</param>
        /// <param name="spacing">The spacing factor. Its X component is used for character spacing, and Y component for line spacing.</param>
        /// <param name="color">The color of the text.</param>
        /// <returns>Size of the drawn string, in pixels.</returns>
        public static Vector2 DrawString([NotNull] this SpriteBatch spriteBatch, [NotNull] DynamicSpriteFont spriteFont, [CanBeNull] string str, Vector2 location, Vector2 maxBounds, Vector2 spacing, Color color) {
            return DrawString(spriteBatch, spriteFont, str, location, maxBounds, Vector2.One, spacing, color);
        }

        /// <summary>
        /// Draws a string with variable line heights.
        /// </summary>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to use.</param>
        /// <param name="spriteFont">The <see cref="DynamicSpriteFont"/> to use.</param>
        /// <param name="str">The text to draw.</param>
        /// <param name="location">The top left location of drawn string.</param>
        /// <param name="maxBounds">Maximum bounds. Set its X and/or Y to 0 to disable the constraint on that dimension.</param>
        /// <param name="scale">The scaling factor.</param>
        /// <param name="spacing">The spacing factor. Its X component is used for character spacing, and Y component for line spacing.</param>
        /// <param name="color">The color of the text.</param>
        /// <returns>Size of the drawn string, in pixels.</returns>
        public static Vector2 DrawString([NotNull] this SpriteBatch spriteBatch, [NotNull] DynamicSpriteFont spriteFont, [CanBeNull] string str, Vector2 location, Vector2 maxBounds, Vector2 scale, Vector2 spacing, Color color) {
            if (string.IsNullOrEmpty(str)) {
                return Vector2.Zero;
            }

            return spriteFont.Draw(spriteBatch, str, location, maxBounds, scale, spacing, color);
        }

        /// <summary>
        /// Draws a string with a fixed line height, using default scale, unconstrained bounds, and font size as line height.
        /// </summary>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to use.</param>
        /// <param name="spriteFont">The <see cref="DynamicSpriteFont"/> to use.</param>
        /// <param name="str">The text to draw.</param>
        /// <param name="location">The top left location of drawn string.</param>
        /// <param name="color">The color of the text.</param>
        /// <returns>Size of the drawn string, in pixels.</returns>
        public static Vector2 DrawString([NotNull] this SpriteBatch spriteBatch, [NotNull] DynamicSpriteFont spriteFont, [CanBeNull] string str, Vector2 location, Color color) {
            return DrawString(spriteBatch, spriteFont, str, location, Vector2.Zero, Vector2.One, 1, 0, color);
        }

        /// <summary>
        /// Draws a string with a fixed line height, using default scale and unconstrained bounds.
        /// </summary>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to use.</param>
        /// <param name="spriteFont">The <see cref="DynamicSpriteFont"/> to use.</param>
        /// <param name="str">The text to draw.</param>
        /// <param name="location">The top left location of drawn string.</param>
        /// <param name="characterSpacing">The character spacing.</param>
        /// <param name="lineHeight">Line height.</param>
        /// <param name="color">The color of the text.</param>
        /// <returns>Size of the drawn string, in pixels.</returns>
        public static Vector2 DrawString([NotNull] this SpriteBatch spriteBatch, [NotNull] DynamicSpriteFont spriteFont, [CanBeNull] string str, Vector2 location, float characterSpacing, float lineHeight, Color color) {
            return DrawString(spriteBatch, spriteFont, str, location, Vector2.Zero, Vector2.One, characterSpacing, lineHeight, color);
        }

        /// <summary>
        /// Draws a string with a fixed line height, using default scale.
        /// </summary>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to use.</param>
        /// <param name="spriteFont">The <see cref="DynamicSpriteFont"/> to use.</param>
        /// <param name="str">The text to draw.</param>
        /// <param name="location">The top left location of drawn string.</param>
        /// <param name="maxBounds">Maximum bounds. Set its X and/or Y to 0 to disable the constraint on that dimension.</param>
        /// <param name="characterSpacing">The character spacing.</param>
        /// <param name="lineHeight">Line height.</param>
        /// <param name="color">The color of the text.</param>
        /// <returns>Size of the drawn string, in pixels.</returns>
        public static Vector2 DrawString([NotNull] this SpriteBatch spriteBatch, [NotNull] DynamicSpriteFont spriteFont, [CanBeNull] string str, Vector2 location, Vector2 maxBounds, float characterSpacing, float lineHeight, Color color) {
            return DrawString(spriteBatch, spriteFont, str, location, maxBounds, Vector2.One, characterSpacing, lineHeight, color);
        }

        /// <summary>
        /// Draws a string with a fixed line height.
        /// </summary>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to use.</param>
        /// <param name="spriteFont">The <see cref="DynamicSpriteFont"/> to use.</param>
        /// <param name="str">The text to draw.</param>
        /// <param name="location">The top left location of drawn string.</param>
        /// <param name="maxBounds">Maximum bounds. Set its X and/or Y to 0 to disable the constraint on that dimension.</param>
        /// <param name="scale">The scaling factor.</param>
        /// <param name="characterSpacing">The character spacing.</param>
        /// <param name="lineHeight">Line height.</param>
        /// <param name="color">The color of the text.</param>
        /// <returns>Size of the drawn string, in pixels.</returns>
        public static Vector2 DrawString([NotNull] this SpriteBatch spriteBatch, [NotNull] DynamicSpriteFont spriteFont, [CanBeNull] string str, Vector2 location, Vector2 maxBounds, Vector2 scale, float characterSpacing, float lineHeight, Color color) {
            if (string.IsNullOrEmpty(str)) {
                return Vector2.Zero;
            }

            return spriteFont.Draw(spriteBatch, str, location, maxBounds, scale, characterSpacing, lineHeight, color);
        }

    }
}
