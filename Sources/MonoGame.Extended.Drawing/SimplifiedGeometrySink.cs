using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Drawing.Geometries;

namespace MonoGame.Extended.Drawing;

[PublicAPI]
public class SimplifiedGeometrySink : Sink
{

    // https://learn.microsoft.com/en-us/windows/win32/api/d2d1/nf-d2d1-id2d1simplifiedgeometrysink-setfillmode
    internal const FillMode DefaultFillMode = FillMode.Alternate;

    internal SimplifiedGeometrySink()
    {
        _mutableFigures = new List<Figure>();
        CurrentFigureElements = new List<GeometryElement>();

        FillMode = DefaultFillMode;
    }

    public void BeginFigure(Vector2 origin, FigureBegin figureBegin)
    {
        EnsureNotClosed();

        if (_hasFigureBegun)
        {
            throw new InvalidOperationException("Geometry sink is in the middle of drawing a figure.");
        }

        _currentOrigin = origin;
        _currentFigureBegin = figureBegin;

        _hasFigureBegun = true;
        _hasFigureEverBegun = true;
    }

    public void EndFigure(FigureEnd figureEnd)
    {
        EnsureNotClosed();
        EnsureFigureHasBegun();

        _hasFigureBegun = false;

        if (CurrentFigureElements.Count > 0)
        {
            var figure = new Figure(_currentOrigin, CurrentFigureElements.ToArray(), _currentFigureBegin, figureEnd);
            _mutableFigures.Add(figure);
            CurrentFigureElements.Clear();
        }
    }

    public void AddLine(Vector2 point)
    {
        EnsureNotClosed();
        EnsureFigureHasBegun();

        CurrentFigureElements.Add(new GeometryElement(point));
    }

    public void AddLines(Vector2[] points)
    {
        EnsureNotClosed();
        EnsureFigureHasBegun();

        foreach (var point in points)
        {
            var elem = new GeometryElement(point);
            CurrentFigureElements.Add(elem);
        }
    }

    public void AddBeziers(BezierSegment[] beziers)
    {
        EnsureNotClosed();
        EnsureFigureHasBegun();

        foreach (var bezier in beziers)
        {
            var elem = new GeometryElement(bezier);
            CurrentFigureElements.Add(elem);
        }
    }

    public FillMode FillMode
    {
        private get => _fillMode;
        set
        {
            if (_hasFigureEverBegun)
            {
                // Conform with Direct2D's behavior
                throw new InvalidOperationException("Cannot set " + nameof(FillMode) + " after " + nameof(BeginFigure) + "() is called.");
            }

            _fillMode = value;
        }
    }

    internal FigureBatch GetFigureBatch()
    {
        if (_frozenFigures is null)
        {
            throw new InvalidOperationException("Cannot retrieve figures when the geometry sink is not closed.");
        }

        return new FigureBatch(_frozenFigures, FillMode);
    }

    internal void InternalAddArc(in ArcSegment arc)
    {
        EnsureNotClosed();
        EnsureFigureHasBegun();

        CurrentFigureElements.Add(new GeometryElement(arc));
    }

    internal void InternalAddBezier(in BezierSegment bezier)
    {
        EnsureNotClosed();
        EnsureFigureHasBegun();

        CurrentFigureElements.Add(new GeometryElement(bezier));
    }

    internal void InternalAddQuadraticBezier(in QuadraticBezierSegment quadraticBezier)
    {
        EnsureNotClosed();
        EnsureFigureHasBegun();

        CurrentFigureElements.Add(new GeometryElement(quadraticBezier));
    }

    internal void InternalAddQuadraticBeziers(QuadraticBezierSegment[] quadraticBeziers)
    {
        EnsureNotClosed();
        EnsureFigureHasBegun();

        foreach (var quadraticBezier in quadraticBeziers)
        {
            var elem = new GeometryElement(quadraticBezier);
            CurrentFigureElements.Add(elem);
        }
    }

    /// <summary>
    /// Only used for <see cref="EllipseGeometry"/>; don't use it for anything else!
    /// </summary>
    internal void AddMathArc(in MathArcSegment arc)
    {
        EnsureNotClosed();
        EnsureFigureHasBegun();

        CurrentFigureElements.Add(new GeometryElement(arc));
    }

    protected override void OnClosed()
    {
        _frozenFigures = _mutableFigures.ToArray();
        _mutableFigures.Clear();
    }

    // ReSharper disable once MemberCanBePrivate.Global
    protected void EnsureFigureHasBegun()
    {
        if (!_hasFigureBegun)
        {
            throw new InvalidOperationException();
        }
    }

    // ReSharper disable once MemberCanBePrivate.Global
    private protected List<GeometryElement> CurrentFigureElements { get; }

    private Vector2 _currentOrigin;
    private FigureBegin _currentFigureBegin;

    private readonly List<Figure> _mutableFigures;
    private bool _hasFigureBegun;
    private bool _hasFigureEverBegun;

    private Figure[]? _frozenFigures;
    private FillMode _fillMode;

}
