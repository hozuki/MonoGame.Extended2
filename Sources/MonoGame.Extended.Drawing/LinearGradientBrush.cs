using System;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.Drawing {
    public sealed class LinearGradientBrush : Brush {

        public LinearGradientBrush([NotNull] DrawingContext context, LinearGradientBrushProperties properties, [NotNull] GradientStopCollection gradientStopCollection)
            : this(context, properties, BrushProperties.Default, gradientStopCollection) {
        }

        public LinearGradientBrush([NotNull] DrawingContext context, LinearGradientBrushProperties properties, BrushProperties brushProperties, [NotNull] GradientStopCollection gradientStopCollection)
            : base(context, LoadEffect, brushProperties) {
            GradientStopCollection = gradientStopCollection;
            Properties = properties;
        }

        public GradientStopCollection GradientStopCollection { get; }

        public LinearGradientBrushProperties Properties { get; }
        
        protected override void RenderInternal(Triangle[] vertices, Effect effect) {
            throw new NotImplementedException();
        }

        private static (Effect Effect, bool IsShared) LoadEffect([NotNull] GraphicsDevice graphicsDevice) {
            throw new NotImplementedException();
        }

    }
}
