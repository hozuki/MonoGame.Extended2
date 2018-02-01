using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.Drawing.Effects {
    internal abstract class BrushEffect : Effect {

        protected BrushEffect([NotNull] GraphicsDevice graphicsDevice, [NotNull] byte[] effectCode)
            : base(graphicsDevice, effectCode) {
            Initialize();
        }

        internal abstract void Apply();

        internal float Opacity {
            get => _opacity.GetValueSingle();
            set => _opacity.SetValue(MathHelper.Clamp(value, 0, 1));
        }

        internal void SetWorldViewProjection(Matrix world, Matrix view, Matrix projection) {
            var wvp = world * view * projection;
            _worldViewProjection.SetValue(wvp);
        }

        internal static readonly Matrix DefaultWorld = Matrix.Identity;

        internal static readonly Matrix DefaultView = Matrix.CreateLookAt(Vector3.UnitZ, Vector3.Zero, Vector3.UnitY);

        private void Initialize() {
            var p = Parameters;

            _opacity = p["g_opacity"];
            _worldViewProjection = p["g_wvp"];
        }

        private EffectParameter _opacity;
        private EffectParameter _worldViewProjection;

    }
}
