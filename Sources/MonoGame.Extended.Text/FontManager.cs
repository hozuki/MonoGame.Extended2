using System.Collections.Generic;
using SharpFont;

namespace MonoGame.Extended.Text;

/// <inheritdoc />
/// <summary>
/// A manager for <see cref="Font"/>s.
/// This class cannot be inherited.
/// </summary>
public sealed class FontManager : DisposableBase
{

    /// <summary>
    /// Creates a new <see cref="FontManager"/> instance.
    /// </summary>
    public FontManager()
    {
        _library = new Library();

        _fonts = new Dictionary<(string, float, int), Font>();
    }

    /// <summary>
    /// Loads a <see cref="Font"/> from a font file.
    /// </summary>
    /// <param name="fontFilePath">Path of the font file.</param>
    /// <param name="fontSize">Requested font size.</param>
    /// <returns>Loaded <see cref="Font"/> object.</returns>
    public Font LoadFont(string fontFilePath, float fontSize)
    {
        return LoadFont(fontFilePath, fontSize, 0);
    }

    /// <summary>
    /// Loads a <see cref="Font"/> from a font file.
    /// </summary>
    /// <param name="fontFilePath">Path of the font file.</param>
    /// <param name="fontSize">Requested font size.</param>
    /// <param name="faceIndex">Font face index in the font file.</param>
    /// <returns>Loaded <see cref="Font"/> object.</returns>
    public Font LoadFont(string fontFilePath, float fontSize, int faceIndex)
    {
        var key = (fontFilePath, fontSize, faceIndex);

        if (_fonts.ContainsKey(key))
        {
            return _fonts[key];
        }

        var font = new Font(this, fontFilePath, fontSize, faceIndex);

        _fonts[key] = font;

        return font;
    }

    /// <summary>
    /// Gets the underlying <see cref="SharpFont.Library"/> object.
    /// </summary>
    internal Library Library => _library;

    protected override void Dispose(bool disposing)
    {
        foreach (var value in _fonts.Values)
        {
            value.Dispose();
        }

        _fonts.Clear();

        _library.Dispose();
    }

    private readonly Library _library;

    private readonly Dictionary<(string FontFile, float FontSize, int FaceIndex), Font> _fonts;

}
