using JetBrains.Annotations;
using Microsoft.Xna.Framework;

namespace MonoGame.Extended.Drawing.Geometries;

[PublicAPI]
public sealed class RectangleGeometry : Geometry
{

    public RectangleGeometry(RectangleF rectangle)
    {
        _rectangle = rectangle;
        Figures = CreateFigures(in rectangle);
    }

    private protected override FigureBatch Figures { get; }

    private static FigureBatch CreateFigures(in RectangleF rectangle)
    {
        var sink = new SimplifiedGeometrySink();

        sink.BeginFigure(new Vector2(rectangle.Left, rectangle.Top), FigureBegin.Filled);
        {
            sink.AddLine(new Vector2(rectangle.Right, rectangle.Top));
            sink.AddLine(new Vector2(rectangle.Right, rectangle.Bottom));
            sink.AddLine(new Vector2(rectangle.Left, rectangle.Bottom));
        }
        sink.EndFigure(FigureEnd.Closed);

        sink.Close();

        return sink.GetFigureBatch();
    }

    private readonly RectangleF _rectangle;

}
