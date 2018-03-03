using System;
using JetBrains.Annotations;
using SkiaSharp;

namespace MonoGame.Extended.Overlay {
    public sealed class Font : DisposableBase, ICloneable {

        internal Font([NotNull] FontManager manager, [NotNull] SKTypeface typeface) {
            _manager = manager;
            _typeface = typeface;
        }

        public bool IsBold { get; set; }

        public float Size {
            get => _size;
            set {
                if (value <= 0) {
                    value = 1;
                }

                _size = value;
            }
        }

        public string FamilyName => _typeface.FamilyName;

        public Font Clone() {
            return _manager.CreateVariance(this, (FontStyle)Typeface.Style);
        }

        internal SKTypeface Typeface => _typeface;

        protected override void Dispose(bool disposing) {
            _typeface?.Dispose();
        }

        private float _size = 15;
        private readonly SKTypeface _typeface;
        private readonly FontManager _manager;

        object ICloneable.Clone() {
            return Clone();
        }

    }
}
