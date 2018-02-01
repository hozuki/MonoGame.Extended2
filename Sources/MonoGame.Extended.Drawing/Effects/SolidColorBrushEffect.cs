using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.Drawing.Effects {
    internal sealed class SolidColorBrushEffect : BrushEffect {

        private SolidColorBrushEffect([NotNull] GraphicsDevice graphicsDevice, [NotNull] byte[] effectCode)
            : base(graphicsDevice, effectCode) {
            Initialize();
        }

        internal override void Apply() {
            CurrentTechnique.Passes[0].Apply();
        }

        internal static SolidColorBrushEffect Create([NotNull] DrawingContext drawingContext) {
            var bytecode = drawingContext.EffectResources.SolidColorBrush.Bytecode;

            return new SolidColorBrushEffect(drawingContext.GraphicsDevice, bytecode);
        }

        internal Vector4 Color {
            get => _color.GetValueVector4();
            set => _color.SetValue(value);
        }

        private void Initialize() {
            var p = Parameters;

            _color = p["g_color"];
        }

        private EffectParameter _color;

    }
}
