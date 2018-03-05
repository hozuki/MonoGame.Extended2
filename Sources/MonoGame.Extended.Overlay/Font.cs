using System;
using JetBrains.Annotations;
using SkiaSharp;

namespace MonoGame.Extended.Overlay {
    public sealed class Font : DisposableBase, ICloneable {

        internal Font([NotNull] FontManager manager, [NotNull] SKTypeface typeface) {
            _manager = manager;
            _typeface = typeface;
        }

        [NotNull]
        public FontManager FontManager => _manager;

        public bool FakeBold { get; set; }

        public float Size {
            get => _size;
            set {
                if (value <= 0) {
                    value = 1;
                }

                _size = value;
            }
        }

        [NotNull]
        public string FamilyName => _typeface.FamilyName;

        [NotNull]
        public Font Clone() {
            return _manager.CreateFontVariance(this, (FontStyle)Typeface.Style);
        }

        [NotNull]
        internal SKTypeface Typeface => _typeface;

        protected override void Dispose(bool disposing) {
            _typeface?.Dispose();
        }

        private float _size = 15;
        [NotNull]
        private readonly SKTypeface _typeface;
        [NotNull]
        private readonly FontManager _manager;

        object ICloneable.Clone() {
            return Clone();
        }

    }
}
