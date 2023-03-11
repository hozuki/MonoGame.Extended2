using System;
using Microsoft.Xna.Framework;
using SharpFont;

namespace MonoGame.Extended.Text;

/// <inheritdoc />
/// <summary>
/// Represents a font. Currently serves as a wrapper of <see cref="T:SharpFont.Face" />.
/// This class cannot be inherited.
/// </summary>
public sealed class Font : DisposableBase
{

    /// <summary>
    /// Creates a new <see cref="Font"/> instance.
    /// </summary>
    /// <param name="manager">The font manager.</param>
    /// <param name="fontFile">Path of the font file.</param>
    /// <param name="size">Font size, in pixels.</param>
    /// <param name="faceIndex">Font face index in the font file.</param>
    internal Font(FontManager manager, string fontFile, float size, int faceIndex)
    {
        if (size <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size), size, "Font size should be greater than zero.");
        }

        _fontFace = manager.Library.NewFace(fontFile, faceIndex);

        _size = size;

        InitializeFontFace(_fontFace, size, 0);
    }

    /// <summary>
    /// Gets the family name of the font. It is often also the display name.
    /// </summary>
    public string FamilyName => _fontFace.FamilyName;

    /// <summary>
    /// Gets the font size.
    /// </summary>
    public float Size => _size;

    /// <summary>
    /// Gets the underlying <see cref="Face"/> object.
    /// </summary>
    internal Face FontFace => _fontFace;

    protected override void Dispose(bool disposing)
    {
        _fontFace?.Dispose();
    }

    /// <summary>
    /// Initializes a <see cref="Face"/>.
    /// </summary>
    /// <param name="fontFace">The font face to initialize.</param>
    /// <param name="fontSize">Font size, in points.</param>
    /// <param name="rotation">Character rotation, in degrees.</param>
    private static void InitializeFontFace(Face fontFace, float fontSize, float rotation)
    {
        fontFace.SetCharSize(0, fontSize, 0, FontDpi);

        rotation = MathHelper.ToRadians(rotation);

        var delta = new FTVector(0, 0);
        var matrix = new FTMatrix();
        {
            matrix.XX = (Fixed16Dot16)(float)Math.Cos(rotation);
            matrix.XY = (Fixed16Dot16)(float)-Math.Sin(rotation);
            matrix.YX = (Fixed16Dot16)(float)Math.Sin(rotation);
            matrix.YY = (Fixed16Dot16)(float)Math.Cos(rotation);
        }

        fontFace.SetTransform(matrix, delta);
    }

    private const uint FontDpi = 96;

    private readonly Face _fontFace;
    private readonly float _size;

}
