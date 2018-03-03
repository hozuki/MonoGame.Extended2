using System;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Overlay.Extensions;
using SkiaSharp;

namespace MonoGame.Extended.Overlay {
    public sealed class Pen : DisposableBase, IPaintProvider {

        public Pen(Color color)
            : this(color, 1) {
        }

        public Pen(Color color, float strokeWidth)
            : this(color, strokeWidth, LineCap.Round, LineJoin.Round, 0) {
        }

        public Pen(Color color, float strokeWidth, LineCap lineCap, LineJoin lineJoin, float miter) {
            var paint = new SKPaint();

            paint.Color = color.ToSKColor();
            paint.IsAntialias = true;
            paint.IsStroke = true;
            paint.StrokeWidth = strokeWidth;
            paint.StrokeCap = Map(lineCap);
            paint.StrokeJoin = Map(lineJoin);
            paint.StrokeMiter = miter;

            Paint = paint;

            StrokeWidth = strokeWidth;
            LineCap = lineCap;
            LineJoin = lineJoin;
            MiterLimit = miter;
        }

        public Pen([NotNull] Brush brush)
            : this(brush, 1) {
        }

        public Pen([NotNull] Brush brush, float strokeWidth)
            : this(brush, strokeWidth, LineCap.Round, LineJoin.Round, 0) {
        }

        public Pen([NotNull] Brush brush, float strokeWidth, LineCap lineCap, LineJoin lineJoin, float miter) {
            var paint = brush.Paint.Clone();

            paint.IsStroke = true;
            paint.StrokeWidth = strokeWidth;
            paint.StrokeCap = Map(lineCap);
            paint.StrokeJoin = Map(lineJoin);
            paint.StrokeMiter = miter;

            Paint = paint;

            StrokeWidth = strokeWidth;
            LineCap = lineCap;
            LineJoin = lineJoin;
            MiterLimit = miter;
        }

        public float StrokeWidth { get; }

        public LineCap LineCap { get; }

        public LineJoin LineJoin { get; }

        public float MiterLimit { get; }

        [NotNull]
        internal SKPaint Paint { get; }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                Paint.Dispose();
            }
        }

        SKPaint IPaintProvider.Paint => Paint;

        private static SKStrokeCap Map(LineCap cap) {
            switch (cap) {
                case LineCap.Round:
                    return SKStrokeCap.Round;
                case LineCap.Square:
                    return SKStrokeCap.Square;
                case LineCap.Butt:
                    return SKStrokeCap.Butt;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cap), cap, null);
            }
        }

        private static SKStrokeJoin Map(LineJoin join) {
            switch (join) {
                case LineJoin.Round:
                    return SKStrokeJoin.Round;
                case LineJoin.Bevel:
                    return SKStrokeJoin.Bevel;
                case LineJoin.Miter:
                    return SKStrokeJoin.Miter;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@join), @join, null);
            }
        }

    }
}
