using System;
using JetBrains.Annotations;

namespace MonoGame.Extended.Drawing {
    public sealed class Pen {

        public Pen([NotNull] Brush brush, float strokeWidth)
        : this(brush, strokeWidth, null) {
        }

        public Pen([NotNull] Brush brush, float strokeWidth, [CanBeNull] StrokeStyle strokeStyle) {
            if (strokeWidth <= 0) {
                throw new ArgumentOutOfRangeException(nameof(strokeWidth), strokeWidth, "Stroke width must be greater than 0.");
            }

            if (strokeStyle == null) {
                strokeStyle = StrokeStyle.DefaultStyle;
            }

            StrokeWidth = strokeWidth;
            StrokeStyle = strokeStyle;
            Brush = brush;
        }

        public float StrokeWidth { get; }

        public StrokeStyle StrokeStyle { get; }

        internal Brush Brush { get; }

    }
}
