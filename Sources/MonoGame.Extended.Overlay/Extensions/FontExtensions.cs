namespace MonoGame.Extended.Overlay.Extensions;

public static class FontExtensions
{

    public static Font CreateFontVariance(this Font baseFont, FontStyle style = FontStyle.Regular)
    {
        Guard.ArgumentNotNull(baseFont, nameof(baseFont));

        return baseFont.FontManager.CreateFontVariance(baseFont, style);
    }

    public static Font CreateFontVariance(this Font baseFont, FontWeight weight = FontWeight.Normal, FontWidth width = FontWidth.Normal, FontSlant slant = FontSlant.Normal)
    {
        Guard.ArgumentNotNull(baseFont, nameof(baseFont));

        return baseFont.FontManager.CreateFont(baseFont.FamilyName, weight, width, slant);
    }

    public static Font CreateFontVariance(this Font baseFont, int weight, int width = (int)FontWidth.Normal, FontSlant slant = FontSlant.Normal)
    {
        Guard.ArgumentNotNull(baseFont, nameof(baseFont));

        return baseFont.FontManager.CreateFont(baseFont.FamilyName, weight, width, slant);
    }

    public static Font CreateFontVariance(this Font baseFont, float newSize, FontStyle style = FontStyle.Regular)
    {
        Guard.ArgumentNotNull(baseFont, nameof(baseFont));

        var font = baseFont.FontManager.CreateFontVariance(baseFont, style);

        font.Size = newSize;

        return font;
    }

}
