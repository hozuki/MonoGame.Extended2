using Microsoft.Xna.Framework;

namespace MonoGame.Extended.Drawing {
    public struct GradientStop {

        public float Position {
            get => _position;
            set => _position = MathHelper.Clamp(value, 0, 1);
        }

        public Color Color { get; set; }

        private float _position;

    }
}
