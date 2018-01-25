using Microsoft.Xna.Framework;

namespace MonoGame.Extended.Drawing {
    public struct BrushProperties {

        public float Opacity { get; set; }

        public Matrix3x2 Transform { get; set; }

        internal static readonly BrushProperties Default = new BrushProperties {
            Opacity = 1,
            Transform = Matrix3x2.Identity
        };

    }
}
