using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Text.Extensions;
using SharpFont;

namespace MonoGame.Extended.Text {
    /// <inheritdoc />
    /// <summary>
    /// Represents a dynamic sprite font.
    /// This class cannot be inherited.
    /// </summary>
    public sealed class DynamicSpriteFont : DisposableBase {

        /// <summary>
        /// Creates a new <see cref="DynamicSpriteFont"/> instance.
        /// </summary>
        /// <param name="graphics">The <see cref="GraphicsDevice"/> to manage underlying textures.</param>
        /// <param name="font">The <see cref="MonoGame.Extended.Text.Font"/> to use.</param>
        public DynamicSpriteFont([NotNull] GraphicsDevice graphics, [NotNull] Font font) {
            _texturesDict = new Dictionary<char, Texture2D>();
            _metricsDict = new Dictionary<char, GlyphMetrics>();
            _graphics = graphics;

            Font = font;
        }

        /// <summary>
        /// The associated <see cref="MonoGame.Extended.Text.Font"/> object.
        /// </summary>
        public Font Font { get; }

        /// <summary>
        /// Measures a string and returns the size when it is drawn with a fixed line height.
        /// </summary>
        /// <param name="str">The text to measure.</param>
        /// <param name="maxBounds">Maximum bounds. Set its X and/or Y to 0 to disable the constraint on that dimension.</param>
        /// <param name="scale">The scaling factor.</param>
        /// <param name="characterSpacing">The character spacing.</param>
        /// <param name="lineHeight">Line height.</param>
        /// <returns>Size of the drawn string, in pixels.</returns>
        public Vector2 MeasureString([NotNull] string str, Vector2 maxBounds, Vector2 scale, float characterSpacing, float lineHeight) {
            if (str == null) {
                throw new ArgumentNullException(nameof(str));
            }

            if (string.IsNullOrEmpty(str)) {
                return Vector2.Zero;
            }

            return Draw(null, str, Vector2.Zero, maxBounds, scale, characterSpacing, lineHeight, Color.White);
        }

        /// <summary>
        /// Measures a string and returns the size when it is drawn with variable line heights.
        /// </summary>
        /// <param name="str">The text to measure.</param>
        /// <param name="maxBounds">Maximum bounds. Set its X and/or Y to 0 to disable the constraint on that dimension.</param>
        /// <param name="scale">The scaling factor.</param>
        /// <param name="spacing">The spacing factor. Its X component is used for character spacing, and Y component for line spacing.</param>
        /// <returns>Size of the drawn string, in pixels.</returns>
        public Vector2 MeasureString([NotNull] string str, Vector2 maxBounds, Vector2 scale, Vector2 spacing) {
            if (str == null) {
                throw new ArgumentNullException(nameof(str));
            }

            if (string.IsNullOrEmpty(str)) {
                return Vector2.Zero;
            }

            return Draw(null, str, Vector2.Zero, maxBounds, scale, spacing, Color.White);
        }

        /// <summary>
        /// Draws a string with a fixed line height.
        /// </summary>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to use.</param>
        /// <param name="str">The text to draw.</param>
        /// <param name="location">The top left location of drawn string.</param>
        /// <param name="maxBounds">Maximum bounds. Set its X and/or Y to 0 to disable the constraint on that dimension.</param>
        /// <param name="scale">The scaling factor.</param>
        /// <param name="characterSpacing">The character spacing.</param>
        /// <param name="lineHeight">Line height.</param>
        /// <param name="color">The color of the text.</param>
        /// <returns>Size of the drawn string, in pixels.</returns>
        internal Vector2 Draw([CanBeNull] SpriteBatch spriteBatch, [NotNull] string str, Vector2 location, Vector2 maxBounds, Vector2 scale, float characterSpacing, float lineHeight, Color color) {
            if (maxBounds.X.Equals(0)) {
                maxBounds.X = float.MaxValue;
            }

            if (maxBounds.Y.Equals(0)) {
                maxBounds.Y = float.MaxValue;
            }

            var font = Font;

            if (lineHeight.Equals(0)) {
                lineHeight = font.Size;
            }

            AddCharInfo('\n', null);

            AddStringInfo(str);

            float actualBoundsX = 0;
            float actualBoundsY;

            float? lastYMin = null;
            float? firstBaseLineY = null;

            var lines = str.Split(LineSeparators, StringSplitOptions.None);
            var currentLineIndex = 0;

            for (var i = 0; i < lines.Length; ++i) {
                var line = lines[i];
                var outOfBounds = false;
                var nextCharIndex = 0;

                do {
                    var currentLineWidth = 0f;
                    var yMax = float.MinValue;
                    var yMin = float.MaxValue;

                    var iterationBegin = nextCharIndex;

                    for (var j = iterationBegin; j < line.Length; ++j) {
                        var ch = line[j];

                        nextCharIndex = j + 1;

                        var metrics = _metricsDict[ch];

                        if (ch != '\r') {
                            float charWidth;

                            if (metrics.Width.ToSingle() > 0) {
                                charWidth = metrics.Width.ToSingle();
                            } else {
                                charWidth = metrics.HorizontalAdvance.ToSingle();
                            }

                            var expectedLineWidth = currentLineWidth + charWidth * scale.X;

                            if (j > 0) {
                                expectedLineWidth += characterSpacing * scale.X;
                            }

                            if (expectedLineWidth > maxBounds.X) {
                                nextCharIndex = j;

                                break;
                            }

                            currentLineWidth = expectedLineWidth;
                        }

                        var yTop = metrics.HorizontalBearingY.ToSingle();
                        var yBottom = yTop - metrics.Height.ToSingle();

                        yMax = Math.Max(yMax, yTop);
                        yMin = Math.Min(yMin, yBottom);
                    }

                    if (nextCharIndex == iterationBegin) {
                        outOfBounds = true;

                        break;
                    }

                    if (firstBaseLineY == null) {
                        firstBaseLineY = yMax;
                    }

                    if ((firstBaseLineY.Value + lineHeight * currentLineIndex - yMin) * scale.Y > maxBounds.Y) {
                        outOfBounds = true;
                    }

                    if (outOfBounds) {
                        if (currentLineIndex == 0) {
                            firstBaseLineY = null;
                        }

                        break;
                    }

                    if (spriteBatch != null) {
                        var currentX = location.X;
                        var currentY = location.Y + (firstBaseLineY.Value + lineHeight * currentLineIndex) * scale.Y;

                        for (var j = iterationBegin; j < nextCharIndex; ++j) {
                            var ch = line[j];

                            if (ch == '\r') {
                                continue;
                            }

                            var metrics = _metricsDict[ch];

                            if (_texturesDict.ContainsKey(ch)) {
                                var texture = _texturesDict[ch];

                                var charX = currentX;
                                var charY = currentY - metrics.HorizontalBearingY.ToSingle() * scale.Y;

                                spriteBatch.Draw(texture, new Vector2(charX, charY), null, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                            }

                            if (metrics.Width.ToSingle() > 0) {
                                currentX += (metrics.Width.ToSingle() + characterSpacing) * scale.X;
                            } else {
                                currentX += (metrics.HorizontalAdvance.ToSingle() + characterSpacing) * scale.X;
                            }
                        }
                    }

                    lastYMin = yMin;

                    actualBoundsX = Math.Max(actualBoundsX, currentLineWidth);

                    ++currentLineIndex;
                } while (nextCharIndex < line.Length);

                if (outOfBounds) {
                    break;
                }
            }

            actualBoundsY = Math.Max(currentLineIndex - 1, 0) * lineHeight * scale.Y;

            if (firstBaseLineY != null) {
                actualBoundsY += firstBaseLineY.Value * scale.Y;
            }

            if (lastYMin != null) {
                actualBoundsY -= lastYMin.Value * scale.Y;
            }

            return new Vector2(actualBoundsX, actualBoundsY);
        }

        /// <summary>
        /// Draws a string with variable line heights.
        /// </summary>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to use.</param>
        /// <param name="str">The text to draw.</param>
        /// <param name="location">The top left location of drawn string.</param>
        /// <param name="maxBounds">Maximum bounds. Set its X and/or Y to 0 to disable the constraint on that dimension.</param>
        /// <param name="scale">The scaling factor.</param>
        /// <param name="spacing">The spacing factor. Its X component is used for character spacing, and Y component for line spacing.</param>
        /// <param name="color">The color of the text.</param>
        /// <returns>Size of the drawn string, in pixels.</returns>
        internal Vector2 Draw([CanBeNull] SpriteBatch spriteBatch, [NotNull] string str, Vector2 location, Vector2 maxBounds, Vector2 scale, Vector2 spacing, Color color) {
            if (maxBounds.X.Equals(0)) {
                maxBounds.X = float.MaxValue;
            }

            if (maxBounds.Y.Equals(0)) {
                maxBounds.Y = float.MaxValue;
            }

            AddCharInfo('\n', null);

            AddStringInfo(str);

            var font = Font;

            float actualBoundsX = 0;
            float actualBoundsY = 0;

            var newLineCharMetrics = _metricsDict['\n'];

            var lines = str.Split(LineSeparators, StringSplitOptions.None);
            var currentLineIndex = 0;

            for (var i = 0; i < lines.Length; ++i) {
                var line = lines[i];
                var outOfBounds = false;
                var nextCharIndex = 0;

                do {
                    var currentLineHeight = newLineCharMetrics.Height.ToSingle();
                    var currentLineWidth = 0f;
                    var yMax = float.MinValue;
                    var yMin = float.MaxValue;

                    var iterationBegin = nextCharIndex;

                    for (var j = iterationBegin; j < line.Length; ++j) {
                        var ch = line[j];

                        nextCharIndex = j + 1;

                        var metrics = _metricsDict[ch];

                        if (ch != '\r') {
                            float charWidth;

                            if (metrics.Width.ToSingle() > 0) {
                                charWidth = metrics.Width.ToSingle();
                            } else {
                                charWidth = metrics.HorizontalAdvance.ToSingle();
                            }

                            var expectedLineWidth = currentLineWidth + charWidth * scale.X;

                            if (j > 0) {
                                expectedLineWidth += spacing.X * scale.X;
                            }

                            if (expectedLineWidth > maxBounds.X) {
                                nextCharIndex = j;

                                break;
                            }

                            currentLineWidth = expectedLineWidth;
                        }

                        var yTop = metrics.HorizontalBearingY.ToSingle();
                        var yBottom = yTop - metrics.Height.ToSingle();

                        yMax = Math.Max(yMax, yTop);
                        yMin = Math.Min(yMin, yBottom);
                    }

                    if (nextCharIndex == iterationBegin) {
                        outOfBounds = true;

                        break;
                    }

                    if (line.Length > 0) {
                        currentLineHeight = (yMax - yMin) * scale.Y;
                    }

                    if (actualBoundsY + currentLineHeight > maxBounds.Y) {
                        outOfBounds = true;
                    }

                    if (outOfBounds) {
                        break;
                    }

                    if (spriteBatch != null) {
                        var currentX = location.X;
                        var currentY = location.Y + actualBoundsY * scale.Y;

                        if (currentLineIndex > 0) {
                            currentY += spacing.Y * scale.Y;
                        }

                        for (var j = iterationBegin; j < nextCharIndex; ++j) {
                            var ch = line[j];

                            if (ch == '\r') {
                                continue;
                            }

                            var metrics = _metricsDict[ch];

                            if (_texturesDict.ContainsKey(ch)) {
                                var texture = _texturesDict[ch];

                                var charX = currentX;
                                var charY = currentY + (yMax - metrics.HorizontalBearingY.ToSingle()) * scale.Y;

                                spriteBatch.Draw(texture, new Vector2(charX, charY), null, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                            }

                            if (metrics.Width.ToSingle() > 0) {
                                currentX += (metrics.Width.ToSingle() + spacing.X) * scale.X;
                            } else {
                                currentX += (metrics.HorizontalAdvance.ToSingle() + spacing.X) * scale.X;
                            }
                        }
                    }

                    actualBoundsX = Math.Max(actualBoundsX, currentLineWidth);
                    actualBoundsY += currentLineHeight + spacing.Y * scale.Y;
                    ++currentLineIndex;
                } while (nextCharIndex < line.Length);

                if (outOfBounds) {
                    break;
                }
            }

            if (actualBoundsY > 0) {
                actualBoundsY -= spacing.Y * scale.Y;
            }

            return new Vector2(actualBoundsX, actualBoundsY);
        }

        protected override void Dispose(bool disposing) {
            foreach (var value in _texturesDict.Values) {
                value.Dispose();
            }

            _texturesDict.Clear();

            _metricsDict.Clear();
        }

        /// <summary>
        /// Add information of characters in a string to cache.
        /// </summary>
        /// <param name="str">The string containing characters to add.</param>
        private void AddStringInfo([NotNull] string str) {
            for (var i = 0; i < str.Length; i++) {
                var ch = str[i];

                if (ch == '\n') {
                    continue;
                }

                AddCharInfo(ch, null);
            }
        }

        /// <summary>
        /// Add the information of a character to cache.
        /// </summary>
        /// <param name="char">The character to add.</param>
        /// <param name="nextChar">Next character. When set to <see langword="null"/>, it means ignoring the next character.</param>
        private void AddCharInfo(char @char, [CanBeNull] char? nextChar) {
            if (_metricsDict.ContainsKey(@char)) {
                return;
            }

            var fontFace = Font.FontFace;
            var glyphIndex = fontFace.GetCharIndex(@char);

            fontFace.LoadGlyph(glyphIndex, LoadFlags.Render, LoadTarget.Normal);

            var glyph = fontFace.Glyph;

            _metricsDict.Add(@char, glyph.Metrics);

            if (glyph.Bitmap.Buffer == IntPtr.Zero) {
                return;
            }

            // Some fonts would have size.X or size.Y being equal to 0, so here we add an one-pixel redundancy.
            var charSize = fontFace.GetCharSize(glyphIndex, glyph.Metrics, nextChar, 1, 1);
            var texture = new Texture2D(_graphics, (int)Math.Round(charSize.X), (int)Math.Round(charSize.Y), false, SurfaceFormat.Color);

            glyph.Bitmap.RenderToTexture(texture);

            _texturesDict.Add(@char, texture);
        }

        private static readonly char[] LineSeparators = { '\n' };

        private readonly Dictionary<char, Texture2D> _texturesDict;
        private readonly Dictionary<char, GlyphMetrics> _metricsDict;
        private readonly GraphicsDevice _graphics;

    }
}
