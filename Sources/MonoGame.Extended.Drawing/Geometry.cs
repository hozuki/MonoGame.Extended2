using System;
using System.Collections.Generic;
using Agg.AdaptiveSubdivision;
using JetBrains.Annotations;
using LibTessDotNet;
using Microsoft.Xna.Framework;

namespace MonoGame.Extended.Drawing {
    public abstract class Geometry : SinkOpener<GeometrySink> {

        public void CombineWith([NotNull] Geometry inputGeometry, CombineMode combineMode, [NotNull] GeometrySink sink) {
            CombineWith(inputGeometry, combineMode, null, sink);
        }

        public void CombineWith([NotNull] Geometry inputGeometry, CombineMode combineMode, [CanBeNull] Matrix3x2? inputGeometryTransform, [NotNull] GeometrySink sink) {
            throw new NotImplementedException();
        }

        public GeometryRelation CompareWith([NotNull] Geometry inputGeometry) {
            return CompareWith(inputGeometry, null, DefaultFlatteningTolerance);
        }

        public GeometryRelation CompareWith([NotNull] Geometry inputGeometry, [CanBeNull] Matrix3x2? inputGeometryTransform) {
            return CompareWith(inputGeometry, inputGeometryTransform, DefaultFlatteningTolerance);
        }

        public GeometryRelation CompareWith([NotNull] Geometry inputGeometry, float flatteningTolerance) {
            return CompareWith(inputGeometry, null, flatteningTolerance);
        }

        public GeometryRelation CompareWith([NotNull] Geometry inputGeometry, [CanBeNull] Matrix3x2? inputGeometryTransform, float flatteningTolerance) {
            throw new NotImplementedException();
        }

        public float ComputeArea() {
            return ComputeArea(null);
        }

        public float ComputeArea([CanBeNull] Matrix3x2? transform) {
            throw new NotImplementedException();
        }

        public float ComputeLength() {
            return ComputeLength(null);
        }

        public float ComputeLength([CanBeNull] Matrix3x2? transform) {
            throw new NotImplementedException();
        }

        public (Vector2 Point, Vector2 Tangent) ComputePointAtLength(float length) {
            return ComputePointAtLength(length, null);
        }

        public (Vector2 Point, Vector2 Tangent) ComputePointAtLength(float length, [CanBeNull] Matrix3x2? transform) {
            throw new NotImplementedException();
        }

        public bool FillContainsPoint(Vector2 point) {
            return FillContainsPoint(point, null);
        }

        public bool FillContainsPoint(Vector2 point, [CanBeNull] Matrix3x2? transform) {
            throw new NotImplementedException();
        }

        public RectangleF GetBounds() {
            return GetBounds(null);
        }

        public RectangleF GetBounds([CanBeNull] Matrix3x2? transform) {
            throw new NotImplementedException();
        }

        public RectangleF GetWidenedBounds(float strokeWidth, [CanBeNull] StrokeStyle strokeStyle) {
            return GetWidenedBounds(strokeWidth, strokeStyle, null);
        }

        public RectangleF GetWidenedBounds(float strokeWidth, [CanBeNull] StrokeStyle strokeStyle, [CanBeNull] Matrix3x2? transform) {
            throw new NotImplementedException();
        }

        public void Outline([NotNull] GeometrySink sink) {
            Outline(null, sink);
        }

        public void Outline([CanBeNull] Matrix3x2? transform, [NotNull] GeometrySink sink) {
            throw new NotImplementedException();
        }

        public void Simplify(GeometrySimplificationOption simplificationOption, [NotNull] GeometrySink sink) {
            Simplify(simplificationOption, null, sink);
        }

        public void Simplify(GeometrySimplificationOption simplificationOption, [CanBeNull] Matrix3x2? transform, [NotNull] GeometrySink sink) {
            throw new NotImplementedException();
        }

        public bool StrokeContainsPoint(Vector2 point, float strokeWidth, [CanBeNull] StrokeStyle strokeStyle) {
            return StrokeContainsPoint(point, strokeWidth, strokeStyle, null);
        }

        public bool StrokeContainsPoint(Vector2 point, float strokeWidth, [CanBeNull] StrokeStyle strokeStyle, [CanBeNull] Matrix3x2? transform) {
            throw new NotImplementedException();
        }

        public void Tessellate([NotNull] TessellationSink sink) {
            Tessellate(null, DefaultFlatteningTolerance, sink);
        }

        public void Tessellate([CanBeNull] Matrix3x2? transform, [NotNull] TessellationSink sink) {
            Tessellate(transform, DefaultFlatteningTolerance, sink);
        }

        public void Tessellate(float flatteningTolerance, [NotNull] TessellationSink sink) {
            Tessellate(null, flatteningTolerance, sink);
        }

        public void Tessellate([CanBeNull] Matrix3x2? transform, float flatteningTolerance, [NotNull] TessellationSink sink) {
            if (!_isUpdateRequired && _triangles != null) {
                sink.AddTriangles(_triangles);
                sink.Close();

                return;
            }

            var figures = _figures;

            if (figures == null || figures.Length == 0) {
                return;
            }

            var tess = new Tess();

            for (var i = 0; i < figures.Length; ++i) {
                var contour = CreateContourFromFigure(transform, flatteningTolerance, figures[i]);
                tess.AddContour(contour);
            }

            WindingRule winding;

            switch (FillMode) {
                case FillMode.Alternate:
                    winding = WindingRule.EvenOdd;
                    break;
                case FillMode.Winding:
                    winding = WindingRule.Positive;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            tess.Tessellate(winding, ElementType.Polygons, 3);

            var indices = tess.Elements;
            var vertices = tess.Vertices;

            var triangles = new Triangle[indices.Length / 3];

            var c = 0;
            for (var i = 0; i < indices.Length; i += 3) {
                var pos1 = vertices[indices[i]].Position;
                var pos2 = vertices[indices[i + 1]].Position;
                var pos3 = vertices[indices[i + 2]].Position;

                var triangle = new Triangle {
                    Point1 = new Vector2(pos1.X, pos1.Y),
                    Point2 = new Vector2(pos2.X, pos2.Y),
                    Point3 = new Vector2(pos3.X, pos3.Y)
                };

                triangles[c] = triangle;
                ++c;
            }

            _triangles = triangles;

            sink.AddTriangles(triangles);
            sink.Close();

            _isUpdateRequired = false;
        }

        public void Widen(float strokeWidth, [CanBeNull] StrokeStyle strokeStyle, [NotNull] GeometrySink sink) {
            Widen(strokeWidth, strokeStyle, null, DefaultFlatteningTolerance, sink);
        }

        public void Widen(float strokeWidth, [CanBeNull] StrokeStyle strokeStyle, [CanBeNull] Matrix3x2? transform, [NotNull] GeometrySink sink) {
            Widen(strokeWidth, strokeStyle, transform, DefaultFlatteningTolerance, sink);
        }

        public void Widen(float strokeWidth, [CanBeNull] StrokeStyle strokeStyle, float flatteningTolerance, [NotNull] GeometrySink sink) {
            Widen(strokeWidth, strokeStyle, null, flatteningTolerance, sink);
        }

        public void Widen(float strokeWidth, [CanBeNull] StrokeStyle strokeStyle, [CanBeNull] Matrix3x2? transform, float flatteningTolerance, [NotNull] GeometrySink sink) {
            throw new NotImplementedException();
        }

        internal void TessellateOutline(float strokeWidth, [CanBeNull] StrokeStyle strokeStyle, [CanBeNull] Matrix3x2? transform, float flatteningTolerance, [NotNull] TessellationSink sink) {
            throw new NotImplementedException();
        }

        internal static readonly float DefaultFlatteningTolerance = 0.05f;

        internal FillMode FillMode => _fillMode;

        protected override GeometrySink CreateSink() {
            return new GeometrySink(this);
        }

        protected override void OnSinkClosed(GeometrySink sink) {
            _isUpdateRequired = true;

            _figures = sink.Figures;
            _fillMode = sink.FillMode;

            base.OnSinkClosed(sink);
        }

        private static ContourVertex[] CreateContourFromFigure([CanBeNull] Matrix3x2? transform, float flatteningTolerance, Figure figure) {
            if (figure.FigureEnd == FigureEnd.Open) {
                throw new InvalidOperationException("Cannot create a contour, which may be filled, from an open figure.");
            }

            var vertices = new List<ContourVertex>();

            var origin = figure.Origin;
            var lastPoint = origin;

            AddPoint(transform, origin, vertices);

            foreach (var element in figure.Elements) {
                switch (element.Type) {
                    case GeometryElementType.Line: {
                            AddPoint(transform, element.LineSegment, vertices);
                            break;
                        }
                    case GeometryElementType.Arc: {
                            var arc = element.ArcSegment;
                            // The unit in ArcSegment is degrees, not radians;
                            // positive value means clockwise rotation (https://msdn.microsoft.com/en-us/library/windows/desktop/dd370900.aspx).

                            var divided = Subdivider.DivideSvgArc(lastPoint.X, lastPoint.Y, arc.Point.X, arc.Point.Y,
                                arc.Size.X, arc.Size.Y, -MathHelper.ToRadians(arc.RotationAngle),
                                arc.ArcSize == ArcSize.Large, arc.SweepDirection == SweepDirection.Clockwise);

                            AddPoints(transform, divided, vertices);
                            lastPoint = arc.Point;
                            break;
                        }
                    case GeometryElementType.Bezier: {
                            var bezier = element.BezierSegment;
                            var divided = Subdivider.DivideBezier(lastPoint.X, lastPoint.Y, bezier.Point1.X, bezier.Point1.Y,
                                bezier.Point2.X, bezier.Point2.Y, bezier.Point3.X, bezier.Point3.Y, flatteningTolerance);

                            AddPoints(transform, divided, vertices);
                            lastPoint = bezier.Point3;
                            break;
                        }
                    case GeometryElementType.QuadraticBezier: {
                            var bezier = element.QuadraticBezierSegment;
                            var divided = Subdivider.DivideQuadraticBezier(lastPoint.X, lastPoint.Y,
                                bezier.Point1.X, bezier.Point1.Y, bezier.Point2.X, bezier.Point2.Y, flatteningTolerance);

                            AddPoints(transform, divided, vertices);
                            lastPoint = bezier.Point2;
                            break;
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            AddPoint(transform, origin, vertices);

            return vertices.ToArray();
        }

        private static void AddPoint([CanBeNull] Matrix3x2? transform, Vector2 point, List<ContourVertex> vertices) {
            ContourVertex vertex;

            if (transform != null) {
                var pt = Matrix3x2.Transform(transform.Value, point);
                vertex = new ContourVertex {
                    Position = new Vec3 {
                        X = pt.X,
                        Y = pt.Y,
                        Z = StandardZ
                    }
                };
            } else {
                vertex = new ContourVertex {
                    Position = new Vec3 {
                        X = point.X,
                        Y = point.Y,
                        Z = StandardZ
                    }
                };
            }

            vertices.Add(vertex);
        }

        private static void AddPoints([CanBeNull] Matrix3x2? transform, [NotNull] Vector2[] points, List<ContourVertex> vertices) {
            if (points.Length < 2) {
                return;
            }

            // Don't add the first point.
            var verts = new ContourVertex[points.Length - 1];

            if (transform != null) {
                for (var i = 1; i < points.Length; ++i) {
                    var pt = Matrix3x2.Transform(transform.Value, points[i]);

                    verts[i - 1] = new ContourVertex {
                        Position = new Vec3 {
                            X = pt.X,
                            Y = pt.Y,
                            Z = StandardZ
                        }
                    };
                }
            } else {
                for (var i = 1; i < points.Length; ++i) {
                    var pt = points[i];

                    verts[i - 1] = new ContourVertex {
                        Position = new Vec3 {
                            X = pt.X,
                            Y = pt.Y,
                            Z = StandardZ
                        }
                    };
                }
            }

            vertices.AddRange(verts);
        }

        private FillMode _fillMode;

        [CanBeNull, ItemNotNull]
        private Figure[] _figures;

        private bool _isUpdateRequired;

        private Triangle[] _triangles;

        private const float StandardZ = 0f;

    }
}
