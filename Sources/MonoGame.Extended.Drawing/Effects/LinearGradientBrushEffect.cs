using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.Drawing.Effects {
    internal sealed class LinearGradientBrushEffect : GradientBrushEffect {

        private LinearGradientBrushEffect([NotNull] GraphicsDevice graphicsDevice, [NotNull] byte[] effectCode)
            : base(graphicsDevice, effectCode) {
            Initialize();
        }

        internal override void Apply() {
            var key = (Gamma, ExtendMode);
            var passName = PassNames[key];

            CurrentTechnique.Passes[passName].Apply();
        }

        internal static LinearGradientBrushEffect Create([NotNull] DrawingContext drawingContext) {
            var bytecode = drawingContext.EffectResources.LinearGradientBrush.Bytecode;

            return new LinearGradientBrushEffect(drawingContext.GraphicsDevice, bytecode);
        }

        internal Vector2 StartPoint {
            get => _startPoint.GetValueVector2();
            set => _startPoint.SetValue(value);
        }

        internal Vector2 EndPoint {
            get => _endPoint.GetValueVector2();
            set => _endPoint.SetValue(value);
        }

        private void Initialize() {
            var p = Parameters;

            _startPoint = p["startPoint"];
            _endPoint = p["endPoint"];
        }

        private static readonly IReadOnlyDictionary<(Gamma, ExtendMode), string> PassNames = new Dictionary<(Gamma, ExtendMode), string> {
            [(Gamma.SRgb, ExtendMode.Clamp)] = "SRgb_Clamp",
            [(Gamma.Linear, ExtendMode.Clamp)] = "Linear_Clamp",
            [(Gamma.SRgb, ExtendMode.Wrap)] = "SRgb_Wrap",
            [(Gamma.Linear, ExtendMode.Wrap)] = "Linear_Wrap",
            [(Gamma.SRgb, ExtendMode.Mirror)] = "SRgb_Mirror",
            [(Gamma.Linear, ExtendMode.Mirror)] = "Linear_Mirror",
        };

        private EffectParameter _startPoint;
        private EffectParameter _endPoint;

    }
}
