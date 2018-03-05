using System;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;

namespace MonoGame.Extended.Overlay {
    public sealed class ColorBlend {

        public ColorBlend()
            : this(2) {
        }

        public ColorBlend(int count) {
            Guard.GreaterThan(count, 1, nameof(count));

            _colors = new Color[count];
            _positions = new float[count];
        }

        [NotNull]
        public Color[] Colors {
            get => _colors;
            set {
                Guard.ArgumentNotNull(value, nameof(value));

                if (value.Length < 2) {
                    throw new ArgumentException("Color array should have at least 2 colors.");
                }

                _colors = value;
            }
        }

        [NotNull]
        public float[] Positions {
            get => _positions;
            set {
                Guard.ArgumentNotNull(value, nameof(value));

                if (value.Length < 2) {
                    throw new ArgumentException("Position array should have at least 2 numbers.");
                }

                for (var i = 0; i < value.Length; ++i) {
                    value[i] = MathHelper.Clamp(value[i], 0, 1);
                }

                _positions = value;
            }
        }

        [NotNull]
        private Color[] _colors;
        [NotNull]
        private float[] _positions;

    }
}
