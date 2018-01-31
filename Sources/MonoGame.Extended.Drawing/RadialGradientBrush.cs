using System;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.Drawing {
    public sealed class RadialGradientBrush : Brush {

        public RadialGradientBrush([NotNull] DrawingContext context, RadialGradientBrushProperties properties, [NotNull] GradientStopCollection gradientStopCollection)
            : this(context, properties, BrushProperties.Default, gradientStopCollection) {
        }

        public RadialGradientBrush([NotNull] DrawingContext context, RadialGradientBrushProperties properties, BrushProperties brushProperties, [NotNull] GradientStopCollection gradientStopCollection)
            : base(context, LoadEffect, brushProperties) {
            GradientStopCollection = gradientStopCollection;
            Properties = properties;
        }

        public GradientStopCollection GradientStopCollection { get; }

        public RadialGradientBrushProperties Properties { get; }
        
        protected override void RenderInternal(Triangle[] vertices, Effect effect) {
            throw new NotImplementedException();
        }

        private static (Effect Effect, bool IsShared) LoadEffect([NotNull] DrawingContext drawingContext) {
            throw new NotImplementedException();
        }

    }
}
