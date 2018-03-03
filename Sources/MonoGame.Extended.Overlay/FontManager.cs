using System.Collections.Generic;
using JetBrains.Annotations;
using SkiaSharp;

namespace MonoGame.Extended.Overlay {
    public sealed class FontManager : DisposableBase {

        [NotNull]
        public Font CreateFont([NotNull] string familyName, FontStyle style = FontStyle.Regular) {
            Guard.NotNullOrEmpty(familyName, nameof(familyName));

            var typeface = SKTypeface.FromFamilyName(familyName, (SKTypefaceStyle)style);

            Guard.NotNull(typeface, nameof(typeface));

            var font = new Font(this, typeface);

            _loadedFonts.Add(font);

            return font;
        }

        [NotNull]
        public Font CreateFont([NotNull] string familyName, FontWeight weight = FontWeight.Normal, FontWidth width = FontWidth.Normal, FontSlant slant = FontSlant.Normal) {
            Guard.NotNullOrEmpty(familyName, nameof(familyName));

            var typeface = SKTypeface.FromFamilyName(familyName, (SKFontStyleWeight)weight, (SKFontStyleWidth)width, (SKFontStyleSlant)slant);

            Guard.NotNull(typeface, nameof(typeface));

            var font = new Font(this, typeface);

            _loadedFonts.Add(font);

            return font;
        }

        [NotNull]
        public Font CreateFont([NotNull] string familyName, int weight, int width = (int)FontWidth.Normal, FontSlant slant = FontSlant.Normal) {
            Guard.NotNullOrEmpty(familyName, nameof(familyName));

            var typeface = SKTypeface.FromFamilyName(familyName, weight, width, (SKFontStyleSlant)slant);

            Guard.NotNull(typeface, nameof(typeface));

            var font = new Font(this, typeface);

            _loadedFonts.Add(font);

            return font;
        }

        [NotNull]
        public Font LoadFont([NotNull] string fileName, int faceIndex = 0) {
            Guard.NotNullOrEmpty(fileName, nameof(fileName));
            Guard.FileExists(fileName);

            var typeface = SKTypeface.FromFile(fileName, faceIndex);

            Guard.NotNull(typeface, nameof(typeface));

            var font = new Font(this, typeface);

            _loadedFonts.Add(font);

            return font;
        }

        [NotNull]
        public Font CreateFontVariance([NotNull] Font baseFont, FontStyle style) {
            var f = SKTypeface.FromTypeface(baseFont.Typeface, (SKTypefaceStyle)style);
            var font = new Font(this, f);

            font.Size = baseFont.Size;
            font.FakeBold = baseFont.FakeBold;

            _loadedFonts.Add(font);

            return font;
        }

        protected override void Dispose(bool disposing) {
            foreach (var font in _loadedFonts) {
                font.Dispose();
            }
        }

        private readonly List<Font> _loadedFonts = new List<Font>();

    }
}
