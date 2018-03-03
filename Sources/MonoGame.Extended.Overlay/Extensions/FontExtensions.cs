using JetBrains.Annotations;

namespace MonoGame.Extended.Overlay.Extensions {
    public static class FontExtensions {

        public static Font CreateFontVariance([NotNull] this Font baseFont, FontStyle style) {
            Guard.ArgumentNotNull(baseFont, nameof(baseFont));

            return baseFont.FontManager.CreateFontVariance(baseFont, style);
        }

        public static Font CreateFontVariance([NotNull] this Font baseFont, FontStyle style, float newSize) {
            Guard.ArgumentNotNull(baseFont, nameof(baseFont));

            var font = baseFont.FontManager.CreateFontVariance(baseFont, style);

            font.Size = newSize;

            return font;
        }

    }
}
