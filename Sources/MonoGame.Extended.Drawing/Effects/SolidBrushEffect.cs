using JetBrains.Annotations;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.Drawing.Effects {
    internal sealed class SolidBrushEffect : BrushEffect {

        private SolidBrushEffect([NotNull] GraphicsDevice graphicsDevice, [NotNull] byte[] effectCode)
            : base(graphicsDevice, effectCode) {
        }

        internal static SolidBrushEffect Create([NotNull] DrawingContext drawingContext) {
            var bytecode = drawingContext.EffectResources.SolidBrush.Bytecode;

            return new SolidBrushEffect(drawingContext.GraphicsDevice, bytecode);
        }

        internal override void Apply() {
            var tech = Techniques[0];
            var pass = tech.Passes[0];

            pass.Apply();
        }

    }
}
