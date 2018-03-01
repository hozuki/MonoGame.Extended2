using System;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Text;
using MonoGame.Extended.Text.Extensions;
using SharpFont;

namespace MonoGame.Extended.Drawing.Geometries {
    public sealed class PathGeometry : Geometry {

        public static PathGeometry CreateFromString([NotNull] Font font, [CanBeNull] string str) {
            var path = new PathGeometry();
            var sink = path.Open();

            CreateFromString(font, str, sink);

            sink.Close();

            return path;
        }

        public static void CreateFromString([NotNull] Font font, [CanBeNull] string str, [NotNull] GeometrySink sink) {
            if (string.IsNullOrEmpty(str)) {
                return;
            }

            Guard.ArgumentNotNull(font, nameof(font));

            var fontFace = font.FontFace;
            const int noError = 0;
            const int factorX = 1 << 10;
            const int factorY = -factorX;

            var currentOrigin = Vector2.Zero;
            var outlineFuncs = new OutlineFuncs(MoveTo, LineTo, QuadraticBezierTo, CubicBezierTo, factorX, 0);

            foreach (var ch in str) {
                if (ch == '\r') {
                    continue;
                }

                if (ch == ' ') {
                    // TODO: hack!
                    currentOrigin.X += FontHelper.PointsToPixels(font.Size) / 2;
                    continue;
                }

                if (ch == '\n') {
                    currentOrigin.Y += FontHelper.PointsToPixels(font.Size);

                    continue;
                }

                var glyphIndex = fontFace.GetCharIndex(ch);
                fontFace.LoadGlyph(glyphIndex, LoadFlags.Render, LoadTarget.Normal);

                var outline = fontFace.Glyph.Outline;

                sink.BeginFigure(currentOrigin);
                outline.Decompose(outlineFuncs, IntPtr.Zero);
                sink.EndFigure(FigureEnd.Closed);

                var metrics = fontFace.Glyph.Metrics;
                var charSize = fontFace.GetCharSize(glyphIndex, metrics, null, 1, 1);
                currentOrigin.X += charSize.X;
            }

            int MoveTo(ref FTVector p2, IntPtr user) {
                sink.EndFigure(FigureEnd.Closed);

                sink.BeginFigure(currentOrigin + new Vector2(factorX * p2.X.ToSingle(), factorY * p2.Y.ToSingle()));

                return noError;
            }

            int LineTo(ref FTVector p2, IntPtr user) {
                sink.AddLine(currentOrigin + new Vector2(factorX * p2.X.ToSingle(), factorY * p2.Y.ToSingle()));

                return noError;
            }

            int QuadraticBezierTo(ref FTVector cp, ref FTVector p2, IntPtr user) {
                var bezier = new QuadraticBezierSegment {
                    Point1 = currentOrigin + new Vector2(factorX * cp.X.ToSingle(), factorY * cp.Y.ToSingle()),
                    Point2 = currentOrigin + new Vector2(factorX * p2.X.ToSingle(), factorY * p2.Y.ToSingle())
                };
                sink.AddQuadraticBezier(bezier);

                return noError;
            }

            int CubicBezierTo(ref FTVector cp1, ref FTVector cp2, ref FTVector p2, IntPtr user) {
                var bezier = new BezierSegment {
                    Point1 = currentOrigin + new Vector2(factorX * cp1.X.ToSingle(), factorY * cp1.Y.ToSingle()),
                    Point2 = currentOrigin + new Vector2(factorX * cp2.X.ToSingle(), factorY * cp2.Y.ToSingle()),
                    Point3 = currentOrigin + new Vector2(factorX * p2.X.ToSingle(), factorY * p2.Y.ToSingle())
                };
                sink.AddBezier(bezier);

                return noError;
            }
        }

    }
}
