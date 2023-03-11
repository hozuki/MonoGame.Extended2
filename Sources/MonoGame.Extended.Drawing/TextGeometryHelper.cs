using System;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Drawing.Geometries;
using MonoGame.Extended.Text;
using MonoGame.Extended.Text.Extensions;
using SharpFont;

namespace MonoGame.Extended.Drawing;

[PublicAPI]
public static class TextGeometryHelper
{

    public static PathGeometry CreatePathGeometryFromString(Font font, string? str)
    {
        var path = new PathGeometry(true);
        var sink = path.Open();

        AddStringContourToPathGeometry(font, str, sink);

        sink.Close();

        return path;
    }

    private static void AddStringContourToPathGeometry(Font font, string? str, GeometrySink sink)
    {
        if (string.IsNullOrEmpty(str))
        {
            return;
        }

        Guard.ArgumentNotNull(font, nameof(font));

        var fontFace = font.FontFace;
        const int factorX = 1 << 10;
        const int factorY = -factorX;

        var currentOrigin = Vector2.Zero;
        var outlineRenderer = new OutlineRenderer(sink, factorX, factorY);

        foreach (var ch in str)
        {
            if (ch is '\r')
            {
                continue;
            }

            if (ch is ' ')
            {
                // TODO: hack!
                currentOrigin.X += FontHelper.PointsToPixels(font.Size) / 2;
                continue;
            }

            if (ch is '\n')
            {
                currentOrigin.Y += FontHelper.PointsToPixels(font.Size);

                continue;
            }

            var glyphIndex = fontFace.GetCharIndex(ch);
            fontFace.LoadGlyph(glyphIndex, LoadFlags.Render, LoadTarget.Normal);

            outlineRenderer.CurrentPosition = currentOrigin;
            outlineRenderer.Render(fontFace.Glyph.Outline);
            currentOrigin = outlineRenderer.CurrentPosition;

            var metrics = fontFace.Glyph.Metrics;
            var charSize = fontFace.GetCharSize(glyphIndex, metrics, null, 1, 1);
            currentOrigin.X += charSize.X;
        }
    }

    private sealed class OutlineRenderer
    {

        public OutlineRenderer(GeometrySink sink, int factorX, int factorY)
        {
            _sink = sink;
            _factorX = factorX;
            _factorY = factorY;
            _currentPosition = Vector2.Zero;
        }

        public void Render(Outline outline)
        {
            _sink.BeginFigure(Vector2.Zero, FigureBegin.Filled);

            using var outlineFuncs = new OutlineFuncs(MoveTo, LineTo, QuadraticBezierTo, CubicBezierTo, _factorX, 0);
            outline.Decompose(outlineFuncs, IntPtr.Zero);

            _sink.EndFigure(FigureEnd.Closed);
        }

        public Vector2 CurrentPosition
        {
            get => _currentPosition;
            set => _currentPosition = value;
        }

        private int MoveTo(ref FTVector p2, IntPtr user)
        {
            _sink.EndFigure(FigureEnd.Closed);

            _sink.BeginFigure(_currentPosition + new Vector2(_factorX * p2.X.ToSingle(), _factorY * p2.Y.ToSingle()), FigureBegin.Filled);

            return NoError;
        }

        private int LineTo(ref FTVector p2, IntPtr user)
        {
            _sink.AddLine(_currentPosition + new Vector2(_factorX * p2.X.ToSingle(), _factorY * p2.Y.ToSingle()));

            return NoError;
        }

        private int QuadraticBezierTo(ref FTVector cp, ref FTVector p2, IntPtr user)
        {
            var bezier = new QuadraticBezierSegment
            {
                Point1 = _currentPosition + new Vector2(_factorX * cp.X.ToSingle(), _factorY * cp.Y.ToSingle()),
                Point2 = _currentPosition + new Vector2(_factorX * p2.X.ToSingle(), _factorY * p2.Y.ToSingle())
            };
            _sink.AddQuadraticBezier(bezier);

            return NoError;
        }

        private int CubicBezierTo(ref FTVector cp1, ref FTVector cp2, ref FTVector p2, IntPtr user)
        {
            var bezier = new BezierSegment
            {
                Point1 = _currentPosition + new Vector2(_factorX * cp1.X.ToSingle(), _factorY * cp1.Y.ToSingle()),
                Point2 = _currentPosition + new Vector2(_factorX * cp2.X.ToSingle(), _factorY * cp2.Y.ToSingle()),
                Point3 = _currentPosition + new Vector2(_factorX * p2.X.ToSingle(), _factorY * p2.Y.ToSingle())
            };
            _sink.AddBezier(bezier);

            return NoError;
        }

        private const int NoError = 0;

        private readonly GeometrySink _sink;
        private Vector2 _currentPosition;
        private readonly int _factorX;
        private readonly int _factorY;

    }

}
