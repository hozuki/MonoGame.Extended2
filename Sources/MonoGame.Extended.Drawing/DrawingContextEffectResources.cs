using JetBrains.Annotations;
using MonoGame.Extended.Drawing.Effects;

namespace MonoGame.Extended.Drawing {
    internal sealed class DrawingContextEffectResources {

        internal DrawingContextEffectResources([NotNull] DrawingContext drawingContext) {
            SolidBrush = EffectResource.CreateSolidBrushEffect(drawingContext);
        }

        internal EffectResource SolidBrush { get; }

    }
}
