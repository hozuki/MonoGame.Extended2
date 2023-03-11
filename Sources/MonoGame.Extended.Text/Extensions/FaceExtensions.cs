using Microsoft.Xna.Framework;
using SharpFont;

namespace MonoGame.Extended.Text.Extensions;

/// <summary>
/// Extension methods for <see cref="Face"/>.
/// </summary>
internal static class FaceExtensions
{

    /// <summary>
    /// Gets the transformed size of a character image in a font face.
    /// </summary>
    /// <param name="fontFace">The font face.</param>
    /// <param name="glyphIndex">The index of the character glyph. Use <see cref="Face.GetCharIndex"/> to retrieve the glyph index.</param>
    /// <param name="glyphMetrics">Glyph metrics.</param>
    /// <param name="nextChar">The next character in the string. Specify a <see cref="char"/> to enable kerning calculation. When set to <see langword="null"/>, the kerning information is not calculated; instead, <see cref="defaultXSpacing"/> is used.</param>
    /// <param name="defaultXSpacing">Default X spacing value.</param>
    /// <param name="defaultYSpacing">Default Y spacing value.</param>
    /// <returns>Transformed size of the image, in pixels.</returns>
    /// <seealso cref="Face.SetCharSize"/>
    internal static Vector2 GetCharSize(this Face fontFace, uint glyphIndex, GlyphMetrics glyphMetrics, char? nextChar, int defaultXSpacing, int defaultYSpacing)
    {
        var currentCharacterWidth = glyphMetrics.Width.ToInt32();

        if (nextChar is not null && fontFace.HasKerning)
        {
            var kerning = fontFace.GetKerning(glyphIndex, fontFace.GetCharIndex(nextChar.Value), KerningMode.Default);

            currentCharacterWidth += kerning.X.ToInt32();
        }
        else
        {
            // We can't get the kerning value, use defaultXSpacing.
            currentCharacterWidth += defaultXSpacing;
        }

        var currentCharacterHeight = glyphMetrics.Height.ToInt32();
        currentCharacterHeight += defaultYSpacing;

        return new Vector2(currentCharacterWidth, currentCharacterHeight);
    }

}
