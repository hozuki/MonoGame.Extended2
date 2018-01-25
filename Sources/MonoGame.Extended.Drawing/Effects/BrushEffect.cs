using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.Drawing.Effects {
    internal abstract class BrushEffect : Effect {

        protected BrushEffect([NotNull] Effect cloneSource)
            : base(cloneSource) {
        }

        protected BrushEffect([NotNull] GraphicsDevice graphicsDevice, [NotNull] byte[] effectCode)
            : base(graphicsDevice, effectCode) {
        }

        protected BrushEffect([NotNull] GraphicsDevice graphicsDevice, [NotNull] byte[] effectCode, int index, int count)
            : base(graphicsDevice, effectCode, index, count) {
        }

        internal abstract void Apply();

        internal void SetOpacity(float opacity) {
            opacity = MathHelper.Clamp(opacity, 0, 1);

            if (_opacity == null) {
                _opacity = Parameters["g_opacity"];
            }

            _opacity.SetValue(opacity);
        }

        internal void SetWorldViewProjection(Matrix world, Matrix view, Matrix projection) {
            if (_worldViewProjection == null) {
                _worldViewProjection = Parameters["g_wvp"];
            }

            var wvp = world * view * projection;
            _worldViewProjection.SetValue(wvp);
        }

        internal static readonly Matrix DefaultWorld = Matrix.Identity;

        internal static readonly Matrix DefaultView = Matrix.CreateLookAt(Vector3.UnitZ, Vector3.Zero, Vector3.UnitY);

        private EffectParameter _opacity;
        private EffectParameter _worldViewProjection;

    }
}
