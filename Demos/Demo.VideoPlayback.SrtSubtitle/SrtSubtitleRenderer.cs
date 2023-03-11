using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Framework.Media;
using StbSharp;

namespace Demo.VideoPlayback.SrtSubtitle;

public class SrtSubtitleRenderer : SubtitleRenderer
{

    static SrtSubtitleRenderer()
    {
    }

    public SrtSubtitleRenderer(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        _spriteBatch = new SpriteBatch(graphicsDevice);
    }

    public int FontBitmapWidth { get; set; } = 1024;

    public int FontBitmapHeight { get; set; } = 1024;

    public int FontSize { get; set; } = 32;

    public int LineSpacing { get; set; } = 36;

    public float Spacing { get; set; } = 0;

    public float MarginX { get; set; } = 10;

    public float MarginY { get; set; } = 10;

    public char? DefaultCharacter { get; set; } = '?';

    public Color Color { get; set; } = Color.White;

    public void ApplyFontFile(string fileName)
    {
        _fontTexture?.Dispose();

        var fontData = File.ReadAllBytes(fileName);
        var tempBitmap = new byte[FontBitmapWidth * FontBitmapHeight];

        var fontBaker = new FontBaker();

        fontBaker.Begin(tempBitmap, FontBitmapWidth, FontBitmapHeight);

        var fontSizeInPx = PointToPixel(FontSize);

        fontBaker.Add(fontData, fontSizeInPx, new[]
        {
            FontBakerCharacterRange.BasicLatin,
            FontBakerCharacterRange.Latin1Supplement,
            FontBakerCharacterRange.LatinExtendedA,
            FontBakerCharacterRange.LatinExtendedB,
//          FontBakerCharacterRange.Cyrillic,
        });

        var charData = fontBaker.End();

        var minOffsetY = float.MaxValue;

        foreach (var kv in charData)
        {
            if (kv.Value.yoff < minOffsetY)
            {
                minOffsetY = kv.Value.yoff;
            }
        }

        var keys = charData.Keys.ToArray();

        foreach (var key in keys)
        {
            var pc = charData[key];

            pc.yoff -= minOffsetY;

            charData[key] = pc;
        }

        var rgba = new Color[FontBitmapWidth * FontBitmapHeight];

        for (var i = 0; i < tempBitmap.Length; ++i)
        {
            var b = tempBitmap[i];

            rgba[i].R = b;
            rgba[i].G = b;
            rgba[i].B = b;
            rgba[i].A = b;
        }

        var fontTexture = new Texture2D(_graphicsDevice, FontBitmapWidth, FontBitmapHeight, false, SurfaceFormat.Color);

        fontTexture.SetData(rgba);

        var glyphBounds = new List<Rectangle>();
        var cropping = new List<Rectangle>();
        var chars = new List<char>();
        var kerning = new List<Vector3>();

        var orderedKeys = charData.Keys.OrderBy(a => a);

        foreach (var key in orderedKeys)
        {
            var character = charData[key];

            var bounds = new Rectangle(
                character.x0,
                character.y0,
                character.x1 - character.x0,
                character.y1 - character.y0);

            glyphBounds.Add(bounds);
            cropping.Add(new Rectangle((int)character.xoff, (int)character.yoff, bounds.Width, bounds.Height));

            chars.Add(key);

            kerning.Add(new Vector3(0, bounds.Width, character.xadvance - bounds.Width));
        }

        var spriteFont = new SpriteFont(fontTexture, glyphBounds, cropping, chars, LineSpacing, Spacing, kerning, DefaultCharacter);

        _charData = charData;
        _fontTexture = fontTexture;
        _spriteFont = spriteFont;
    }

    public void LoadFromFile(string fileName, Encoding encoding)
    {
        var content = File.ReadAllText(fileName, encoding);

        LoadFromString(content);
    }

    public void LoadFromFile(string fileName)
    {
        var content = File.ReadAllText(fileName);

        LoadFromString(content);
    }

    public void LoadFromString(string content)
    {
        var parsed = SrtParser.Parse(content);

        _entries = parsed;
    }

    public override void Render(TimeSpan time, RenderTarget2D texture)
    {
        var charData = _charData;

        if (charData == null)
        {
            throw new InvalidOperationException("A font file needs to be loaded first");
        }

        var entries = _entries;

        if (entries == null)
        {
            throw new InvalidOperationException("A subtitle file needs to be loaded first");
        }

        var appearingSubtitles = new List<SrtEntry>();

        foreach (var entry in entries)
        {
            if (entry.Start <= time && time <= entry.End)
            {
                appearingSubtitles.Add(entry);
            }
        }

        if (appearingSubtitles.Count == 0)
        {
            return;
        }

        var font = _spriteFont;
        var batch = _spriteBatch;

        Debug.Assert(font != null);
        Debug.Assert(batch != null);

        // Measure and draw strings
        // TODO: An incremental search & apply algorithm can be used here
        var sizes = new List<Vector2>();

        foreach (var entry in appearingSubtitles)
        {
            var size = font.MeasureString(entry.Text);

            sizes.Add(size);
        }

        var positions = new List<Vector2>();
        var textureSize = new Vector2(texture.Width, texture.Height);
        var lastY = textureSize.Y;

        foreach (var size in sizes)
        {
            var x = (textureSize.X - MarginX - MarginX - size.X) / 2;
            var y = lastY - size.Y - LineSpacing - MarginY;

            positions.Add(new Vector2(x, y));

            lastY -= size.Y + LineSpacing;
        }

        batch.Begin();

        for (var i = 0; i < appearingSubtitles.Count; ++i)
        {
            var entry = appearingSubtitles[i];

            batch.DrawString(font, entry.Text, positions[i], Color);
        }

        batch.End();
    }

    protected override void Dispose(bool disposing)
    {
        _spriteBatch.Dispose();
        _fontTexture?.Dispose();
        _fontTexture = null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float PointToPixel(float pt)
    {
        return pt * 4 / 3;
    }

    private readonly GraphicsDevice _graphicsDevice;

    private readonly SpriteBatch _spriteBatch;

    private Texture2D? _fontTexture;

    private SpriteFont? _spriteFont;

    private IReadOnlyDictionary<char, StbTrueType.stbtt_packedchar>? _charData;

    private SrtEntry[]? _entries;

}
