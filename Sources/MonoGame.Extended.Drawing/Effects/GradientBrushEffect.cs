using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.Drawing.Effects {
    internal abstract class GradientBrushEffect : BrushEffect {

        protected GradientBrushEffect([NotNull] GraphicsDevice graphicsDevice, [NotNull] byte[] effectCode)
            : base(graphicsDevice, effectCode) {
            Initialize();
        }

        internal void SetGradientStops([NotNull] GradientStop[] gradientStops) {
            var colors = new Vector4[GradientStopCollection.MaximumGradientStops];
            var positions = new float[GradientStopCollection.MaximumGradientStops];

            for (var i = 0; i < gradientStops.Length; ++i) {
                var gs = gradientStops[i];
                colors[i] = gs.Color.ToVector4();
                positions[i] = gs.Position;
            }

            _gradientStopColors.SetValue(colors);
            _gradientStopPositions.SetValue(positions);
            _numGradientStops.SetValue(gradientStops.Length);
        }

        internal Gamma Gamma { get; set; }

        internal ExtendMode ExtendMode { get; set; }

        private void Initialize() {
            var p = Parameters;

            _gradientStopColors = p["g_gradientStopColors"];
            _gradientStopPositions = p["g_gradientStopPositions"];
            _numGradientStops = p["g_numGradientStops"];
        }

        private EffectParameter _gradientStopColors;
        private EffectParameter _gradientStopPositions;
        private EffectParameter _numGradientStops;

    }
}
