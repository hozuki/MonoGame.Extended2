using System.Collections.Generic;
using JetBrains.Annotations;
using SkiaSharp;

namespace MonoGame.Extended.Overlay {
    public sealed class FontManager : DisposableBase {

        public Font LoadFont([NotNull] string fileName, int faceIndex = 0) {
            var typeface = SKTypeface.FromFile(fileName, faceIndex);
            var font = new Font(this, typeface);

            _loadedFonts.Add(font);

            return font;
        }

        public Font CreateVariance([NotNull] Font baseFont, FontStyle style) {
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
