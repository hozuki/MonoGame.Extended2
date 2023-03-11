using JetBrains.Annotations;
using Microsoft.Xna.Framework;

namespace MonoGame.Extended.Drawing.Geometries;

// Note: there is no interface named "ID2D1LineGeometry".
[PublicAPI]
public sealed class LineGeometry : Geometry
{

    public LineGeometry(Vector2 point1, Vector2 point2)
    {
        _point1 = point1;
        _point2 = point2;
        Figures = CreateFigures(in point1, in point2);
    }

    private protected override FigureBatch Figures { get; }

    private static FigureBatch CreateFigures(in Vector2 point1, in Vector2 point2)
    {
        var sink = new SimplifiedGeometrySink();

        sink.BeginFigure(point1, FigureBegin.Filled);
        {
            sink.AddLine(point2);
        }
        sink.EndFigure(FigureEnd.Open);

        sink.Close();

        return sink.GetFigureBatch();
    }

    private readonly Vector2 _point1;
    private readonly Vector2 _point2;

}
