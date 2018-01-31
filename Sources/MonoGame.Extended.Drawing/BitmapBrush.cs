using System;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.Drawing {
    public sealed class BitmapBrush : Brush {

        public BitmapBrush([NotNull] DrawingContext context, [NotNull] Texture2D bitmap, BitmapBrushProperties properties)
            : this(context, bitmap, properties, BrushProperties.Default) {
        }

        public BitmapBrush([NotNull] DrawingContext context, [NotNull] Texture2D bitmap, BitmapBrushProperties properties, BrushProperties brushProperties)
            : base(context, LoadEffect, brushProperties) {
            Bitmap = bitmap;
            Properties = properties;
        }

        public Texture2D Bitmap { get; }

        public BitmapBrushProperties Properties { get; }

        protected override void RenderInternal(Triangle[] vertices, Effect effect) {
            throw new NotImplementedException();
        }

        private static (Effect Effect, bool IsShared) LoadEffect([NotNull] DrawingContext drawingContext) {
            throw new NotImplementedException();
        }

    }
}
