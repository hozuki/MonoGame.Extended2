using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;

namespace MonoGame.Extended.Drawing {
    public sealed class GeometrySink : Sink {

        internal GeometrySink(Geometry geometry) {
            _geometry = geometry;
            _figures = new List<Figure>();
            _currentFigure = new List<GeometryElement>();
        }

        public FillMode FillMode { get; set; }

        public void BeginFigure(Vector2 origin) {
            EnsureNotClosed();

            if (_hasFigureBegun) {
                throw new InvalidOperationException();
            }

            _currentOrigin = origin;

            _hasFigureBegun = true;
        }

        public void EndFigure(FigureEnd figureEnd) {
            EnsureNotClosed();
            EnsureFigureBegun();

            _hasFigureBegun = false;

            if (_currentFigure.Count > 0) {
                var figure = new Figure(_currentOrigin, _currentFigure.ToArray(), figureEnd);

                _figures.Add(figure);
            }

            _currentFigure.Clear();
        }

        public void AddLine(Vector2 point) {
            EnsureNotClosed();
            EnsureFigureBegun();

            _currentFigure.Add(new GeometryElement(point));
        }

        public void AddLines([NotNull] Vector2[] points) {
            EnsureNotClosed();
            EnsureFigureBegun();

            for (var i = 0; i < points.Length; ++i) {
                var elem = new GeometryElement(points[i]);
                _currentFigure.Add(elem);
            }
        }

        public void AddArc(ArcSegment arc) {
            EnsureNotClosed();
            EnsureFigureBegun();

            _currentFigure.Add(new GeometryElement(arc));
        }

        public void AddBezier(BezierSegment bezier) {
            EnsureNotClosed();
            EnsureFigureBegun();

            _currentFigure.Add(new GeometryElement(bezier));
        }

        public void AddBeziers([NotNull] BezierSegment[] beziers) {
            EnsureNotClosed();
            EnsureFigureBegun();

            for (var i = 0; i < beziers.Length; ++i) {
                var elem = new GeometryElement(beziers[i]);
                _currentFigure.Add(elem);
            }
        }

        public void AddQuadraticBezier(QuadraticBezierSegment quadraticBezier) {
            EnsureNotClosed();
            EnsureFigureBegun();

            _currentFigure.Add(new GeometryElement(quadraticBezier));
        }

        public void AddQuadraticBeziers([NotNull] QuadraticBezierSegment[] quadraticBeziers) {
            EnsureNotClosed();
            EnsureFigureBegun();

            for (var i = 0; i < quadraticBeziers.Length; ++i) {
                var elem = new GeometryElement(quadraticBeziers[i]);
                _currentFigure.Add(elem);
            }
        }

        internal Figure[] Figures { get; private set; }

        protected override void OnClosed() {
            Figures = _figures.ToArray();
            _geometry.CloseSink(this);

            base.OnClosed();
        }

        private void EnsureFigureBegun() {
            if (!_hasFigureBegun) {
                throw new InvalidOperationException();
            }
        }

        private Vector2 _currentOrigin;
        private readonly List<GeometryElement> _currentFigure;

        private readonly List<Figure> _figures;
        private bool _hasFigureBegun;

        private readonly Geometry _geometry;

    }
}
