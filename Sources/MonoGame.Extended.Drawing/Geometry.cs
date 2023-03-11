using System;
using System.Collections.Generic;
using Agg.AdaptiveSubdivision;
using JetBrains.Annotations;
using LibTessDotNet;
using Microsoft.Xna.Framework;

namespace MonoGame.Extended.Drawing;

[PublicAPI]
public abstract class Geometry
{

    public void CombineWith(Geometry inputGeometry, CombineMode combineMode, GeometrySink sink)
    {
        CombineWith(inputGeometry, combineMode, null, sink);
    }

    public void CombineWith(Geometry inputGeometry, CombineMode combineMode, Matrix3x2? inputGeometryTransform, GeometrySink sink)
    {
        throw new NotImplementedException();
    }

    public GeometryRelation CompareWith(Geometry inputGeometry)
    {
        return CompareWith(inputGeometry, null, DefaultFlatteningTolerance);
    }

    public GeometryRelation CompareWith(Geometry inputGeometry, Matrix3x2? inputGeometryTransform)
    {
        return CompareWith(inputGeometry, inputGeometryTransform, DefaultFlatteningTolerance);
    }

    public GeometryRelation CompareWith(Geometry inputGeometry, float flatteningTolerance)
    {
        return CompareWith(inputGeometry, null, flatteningTolerance);
    }

    public GeometryRelation CompareWith(Geometry inputGeometry, Matrix3x2? inputGeometryTransform, float flatteningTolerance)
    {
        throw new NotImplementedException();
    }

    public float ComputeArea()
    {
        return ComputeArea(null);
    }

    public float ComputeArea(Matrix3x2? transform)
    {
        throw new NotImplementedException();
    }

    public float ComputeLength()
    {
        return ComputeLength(null);
    }

    public float ComputeLength(Matrix3x2? transform)
    {
        throw new NotImplementedException();
    }

    public (Vector2 Point, Vector2 Tangent) ComputePointAtLength(float length)
    {
        return ComputePointAtLength(length, null);
    }

    public (Vector2 Point, Vector2 Tangent) ComputePointAtLength(float length, Matrix3x2? transform)
    {
        throw new NotImplementedException();
    }

    public bool FillContainsPoint(Vector2 point)
    {
        return FillContainsPoint(point, null);
    }

    public bool FillContainsPoint(Vector2 point, Matrix3x2? transform)
    {
        throw new NotImplementedException();
    }

    public RectangleF GetBounds()
    {
        return GetBounds(null);
    }

    public RectangleF GetBounds(Matrix3x2? transform)
    {
        throw new NotImplementedException();
    }

    public RectangleF GetWidenedBounds(float strokeWidth, StrokeStyle? strokeStyle)
    {
        return GetWidenedBounds(strokeWidth, strokeStyle, null);
    }

    public RectangleF GetWidenedBounds(float strokeWidth, StrokeStyle? strokeStyle, Matrix3x2? transform)
    {
        throw new NotImplementedException();
    }

    public void Outline(GeometrySink sink)
    {
        Outline(null, sink);
    }

    public void Outline(Matrix3x2? transform, GeometrySink sink)
    {
        throw new NotImplementedException();
    }

    public void Simplify(GeometrySimplificationOption simplificationOption, GeometrySink sink)
    {
        Simplify(simplificationOption, null, sink);
    }

    public void Simplify(GeometrySimplificationOption simplificationOption, Matrix3x2? transform, GeometrySink sink)
    {
        throw new NotImplementedException();
    }

    public bool StrokeContainsPoint(Vector2 point, float strokeWidth, StrokeStyle? strokeStyle)
    {
        return StrokeContainsPoint(point, strokeWidth, strokeStyle, null);
    }

    public bool StrokeContainsPoint(Vector2 point, float strokeWidth, StrokeStyle? strokeStyle, Matrix3x2? transform)
    {
        throw new NotImplementedException();
    }

    public void Tessellate(TessellationSink sink)
    {
        Tessellate(null, DefaultFlatteningTolerance, sink);
    }

    public void Tessellate(Matrix3x2? transform, TessellationSink sink)
    {
        Tessellate(transform, DefaultFlatteningTolerance, sink);
    }

    public void Tessellate(float flatteningTolerance, TessellationSink sink)
    {
        Tessellate(null, flatteningTolerance, sink);
    }

    public void Tessellate(Matrix3x2? transform, float flatteningTolerance, TessellationSink sink)
    {
        Tessellate(transform, flatteningTolerance, sink, false);
    }

    public void Widen(float strokeWidth, StrokeStyle? strokeStyle, GeometrySink sink)
    {
        Widen(strokeWidth, strokeStyle, null, DefaultFlatteningTolerance, sink);
    }

    public void Widen(float strokeWidth, StrokeStyle? strokeStyle, Matrix3x2? transform, GeometrySink sink)
    {
        Widen(strokeWidth, strokeStyle, transform, DefaultFlatteningTolerance, sink);
    }

    public void Widen(float strokeWidth, StrokeStyle? strokeStyle, float flatteningTolerance, GeometrySink sink)
    {
        Widen(strokeWidth, strokeStyle, null, flatteningTolerance, sink);
    }

    public void Widen(float strokeWidth, StrokeStyle? strokeStyle, Matrix3x2? transform, float flatteningTolerance, GeometrySink sink)
    {
        throw new NotImplementedException();
    }

    internal void TessellateOutline(float strokeWidth, StrokeStyle? strokeStyle, Matrix3x2? transform, float flatteningTolerance, TessellationSink sink)
    {
        throw new NotImplementedException();
    }

    internal static readonly float DefaultFlatteningTolerance = 0.05f;

    internal Triangle[] TessellateForFillGeometry()
    {
        if (_tessellationResultCache is not null)
        {
            return _tessellationResultCache;
        }

        var mesh = new Mesh();
        var tessSink = mesh.Open();

        Tessellate(Matrix3x2.Identity, DefaultFlatteningTolerance, tessSink, true);

        tessSink.Close();

        return tessSink.Triangles;
    }

    protected void InvalidateTesselationResultCache()
    {
        _tessellationResultCache = null;
    }

    private protected abstract FigureBatch Figures { get; }

    private void Tessellate(Matrix3x2? transform, float flatteningTolerance, TessellationSink sink, bool isFillGeometryCall)
    {
        if (_tessellationResultCache is not null)
        {
            if (isFillGeometryCall)
            {
                sink.SetFrozenTriangles(_tessellationResultCache);
            }
            else
            {
                sink.AddTriangles(_tessellationResultCache);
            }

            return;
        }

        var figureCollection = Figures;

        if (figureCollection is null)
        {
            throw new InvalidOperationException("Cannot tessellate with null figure list. Is current sink correctly closed?");
        }

        if (figureCollection.Figures.Length == 0)
        {
            return;
        }

        var tess = new Tess();

        foreach (var figure in figureCollection.Figures)
        {
            var contour = CreateContourFromFigure(transform, flatteningTolerance, figure);
            tess.AddContour(contour);
        }

        var figureFillMode = figureCollection.FillMode;
        var winding = figureFillMode switch
        {
            FillMode.Alternate => WindingRule.EvenOdd,
            FillMode.Winding => WindingRule.Positive,
            _ => throw new ArgumentOutOfRangeException(nameof(FillMode), figureFillMode, null)
        };

        tess.Tessellate(winding, ElementType.Polygons, 3);

        var indices = tess.Elements;
        var vertices = tess.Vertices;

        var triangles = new Triangle[indices.Length / 3];

        var c = 0;
        for (var i = 0; i < indices.Length; i += 3)
        {
            var pos1 = vertices[indices[i]].Position;
            var pos2 = vertices[indices[i + 1]].Position;
            var pos3 = vertices[indices[i + 2]].Position;

            var triangle = new Triangle
            {
                Point1 = new Vector2(pos1.X, pos1.Y),
                Point2 = new Vector2(pos2.X, pos2.Y),
                Point3 = new Vector2(pos3.X, pos3.Y)
            };

            triangles[c] = triangle;
            ++c;
        }

        sink.AddTriangles(triangles);
        _tessellationResultCache = triangles;
    }

    private static ContourVertex[] CreateContourFromFigure(Matrix3x2? transform, float flatteningTolerance, Figure figure)
    {
        if (figure.FigureEnd == FigureEnd.Open)
        {
            throw new InvalidOperationException("Cannot create a contour, which may be filled, from an open figure.");
        }

        var vertices = new List<ContourVertex>();

        var origin = figure.Origin;
        var lastPoint = origin;

        AddPoint(transform, origin, vertices);

        foreach (var element in figure.Elements)
        {
            switch (element.Type)
            {
                case GeometryElementType.Line:
                {
                    AddPoint(transform, element.LineSegment, vertices);
                    lastPoint = element.LineSegment;
                    break;
                }
                case GeometryElementType.Arc:
                {
                    var arc = element.ArcSegment;
                    // The unit in ArcSegment is degrees, not radians;
                    // positive value means clockwise rotation (https://msdn.microsoft.com/en-us/library/windows/desktop/dd370900.aspx).

                    var divided = Subdivider.DivideSvgArc(lastPoint.X, lastPoint.Y, arc.Point.X, arc.Point.Y,
                        arc.Size.X, arc.Size.Y, -MathHelper.ToRadians(arc.RotationAngle),
                        arc.ArcSize == ArcSize.Large, arc.SweepDirection == SweepDirection.Clockwise,
                        flatteningTolerance);

                    AddPoints(transform, divided, vertices);
                    lastPoint = arc.Point;
                    break;
                }
                case GeometryElementType.Bezier:
                {
                    var bezier = element.BezierSegment;
                    var divided = Subdivider.DivideBezier(lastPoint.X, lastPoint.Y, bezier.Point1.X, bezier.Point1.Y,
                        bezier.Point2.X, bezier.Point2.Y, bezier.Point3.X, bezier.Point3.Y, flatteningTolerance);

                    AddPoints(transform, divided, vertices);
                    lastPoint = bezier.Point3;
                    break;
                }
                case GeometryElementType.QuadraticBezier:
                {
                    var bezier = element.QuadraticBezierSegment;
                    var divided = Subdivider.DivideQuadraticBezier(lastPoint.X, lastPoint.Y,
                        bezier.Point1.X, bezier.Point1.Y, bezier.Point2.X, bezier.Point2.Y, flatteningTolerance);

                    AddPoints(transform, divided, vertices);
                    lastPoint = bezier.Point2;
                    break;
                }
                case GeometryElementType.MathArc:
                {
                    var mathArc = element.MathArcSegment;
                    var divided = Subdivider.DivideArc(mathArc.Center.X, mathArc.Center.Y, mathArc.Radius.X, mathArc.Radius.Y,
                        MathHelper.ToRadians(mathArc.StartAngle), MathHelper.ToRadians(mathArc.SweepAngle), MathHelper.ToRadians(mathArc.RotationAngle),
                        flatteningTolerance);

                    AddPoints(transform, divided, vertices);
                    // Not setting lastPoint because we don't need it. See notes of MathArcSegment.
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(element.Type), element.Type, null);
            }
        }

        AddPoint(transform, origin, vertices);

        if (figure.FigureBegin == FigureBegin.Hollow)
        {
            // Reverse the vertex order so the tessellator can recognize this
            vertices.Reverse();
        }

        return vertices.ToArray();
    }

    private static void AddPoint(Matrix3x2? transform, Vector2 point, List<ContourVertex> vertices)
    {
        ContourVertex vertex;

        if (transform.HasValue)
        {
            var pt = Matrix3x2.Transform(transform.Value, point);
            vertex = new ContourVertex
            {
                Position = new Vec3
                {
                    X = pt.X,
                    Y = pt.Y,
                    Z = StandardZ,
                }
            };
        }
        else
        {
            vertex = new ContourVertex
            {
                Position = new Vec3
                {
                    X = point.X,
                    Y = point.Y,
                    Z = StandardZ,
                }
            };
        }

        vertices.Add(vertex);
    }

    private static void AddPoints(Matrix3x2? transform, Vector2[] points, List<ContourVertex> vertices)
    {
        if (points.Length < 2)
        {
            return;
        }

        // Don't add the first point.
        var verts = new ContourVertex[points.Length - 1];

        if (transform != null)
        {
            for (var i = 1; i < points.Length; ++i)
            {
                var pt = Matrix3x2.Transform(transform.Value, points[i]);

                verts[i - 1] = new ContourVertex
                {
                    Position = new Vec3
                    {
                        X = pt.X,
                        Y = pt.Y,
                        Z = StandardZ,
                    }
                };
            }
        }
        else
        {
            for (var i = 1; i < points.Length; ++i)
            {
                var pt = points[i];

                verts[i - 1] = new ContourVertex
                {
                    Position = new Vec3
                    {
                        X = pt.X,
                        Y = pt.Y,
                        Z = StandardZ,
                    }
                };
            }
        }

        vertices.AddRange(verts);
    }

    private const float StandardZ = 0.0f;

    private Triangle[]? _tessellationResultCache;

}
