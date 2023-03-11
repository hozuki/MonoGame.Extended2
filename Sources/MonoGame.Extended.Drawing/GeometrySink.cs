using JetBrains.Annotations;
using MonoGame.Extended.Drawing.Geometries;

namespace MonoGame.Extended.Drawing;

[PublicAPI]
public sealed class GeometrySink : SimplifiedGeometrySink
{

    internal GeometrySink(PathGeometry geometry)
    {
        _geometry = geometry;
    }

    public void AddArc(ArcSegment arc)
    {
        InternalAddArc(in arc);
    }

    public void AddBezier(BezierSegment bezier)
    {
        InternalAddBezier(in bezier);
    }

    public void AddQuadraticBezier(QuadraticBezierSegment quadraticBezier)
    {
        InternalAddQuadraticBezier(in quadraticBezier);
    }

    public void AddQuadraticBeziers(QuadraticBezierSegment[] quadraticBeziers)
    {
        InternalAddQuadraticBeziers(quadraticBeziers);
    }

    protected override void OnClosed()
    {
        base.OnClosed();
        _geometry.Close(this);
    }

    private readonly PathGeometry _geometry;

}
