using JetBrains.Annotations;
using Microsoft.Xna.Framework;

namespace MonoGame.Extended.Drawing.Geometries;

[PublicAPI]
public sealed class EllipseGeometry : Geometry
{

    public EllipseGeometry(Ellipse ellipse)
    {
        _ellipse = ellipse;
        Figures = CreateFigures(in ellipse);
    }

    private protected override FigureBatch Figures { get; }

    private static FigureBatch CreateFigures(in Ellipse ellipse)
    {
        var sink = new SimplifiedGeometrySink();

        var pt = ellipse.Point + new Vector2(ellipse.RadiusX, 0);

        sink.BeginFigure(pt, FigureBegin.Filled);
        {
            var segment = new MathArcSegment
            {
                Center = ellipse.Point,
                Radius = new Vector2(ellipse.RadiusX, ellipse.RadiusY),
                RotationAngle = 0,
                StartAngle = 0,
                SweepAngle = 360,
            };
            sink.AddMathArc(segment);
        }
        sink.EndFigure(FigureEnd.Closed);

        sink.Close();

        return sink.GetFigureBatch();
    }

    private readonly Ellipse _ellipse;

}
