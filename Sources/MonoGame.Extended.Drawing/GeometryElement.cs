using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Drawing.Geometries;

namespace MonoGame.Extended.Drawing;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
internal struct GeometryElement
{

    public GeometryElement(Vector2 lineSegment)
        : this()
    {
        Type = GeometryElementType.Line;
        LineSegment = lineSegment;
    }

    public GeometryElement(ArcSegment arcSegment)
        : this()
    {
        Type = GeometryElementType.Arc;
        ArcSegment = arcSegment;
    }

    public GeometryElement(BezierSegment bezierSegment)
        : this()
    {
        Type = GeometryElementType.Bezier;
        BezierSegment = bezierSegment;
    }

    public GeometryElement(QuadraticBezierSegment quadraticBezierSegment)
        : this()
    {
        Type = GeometryElementType.QuadraticBezier;
        QuadraticBezierSegment = quadraticBezierSegment;
    }

    /// <summary>
    /// Only used for <see cref="EllipseGeometry"/>; don't use it for anything else!
    /// </summary>
    public GeometryElement(MathArcSegment mathArcSegment)
        : this()
    {
        Type = GeometryElementType.MathArc;
        MathArcSegment = mathArcSegment;
    }

    [field: FieldOffset(0)]
    public GeometryElementType Type { get; set; }

    [field: FieldOffset(4)]
    public Vector2 LineSegment { get; set; }

    [field: FieldOffset(4)]
    public ArcSegment ArcSegment { get; set; }

    [field: FieldOffset(4)]
    public BezierSegment BezierSegment { get; set; }

    [field: FieldOffset(4)]
    public QuadraticBezierSegment QuadraticBezierSegment { get; set; }

    /// <summary>
    /// Only used for <see cref="EllipseGeometry"/>; don't use it for anything else!
    /// </summary>
    [field: FieldOffset(4)]
    public MathArcSegment MathArcSegment { get; set; }

}
