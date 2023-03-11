using Microsoft.Xna.Framework;

namespace MonoGame.Extended.Drawing.Geometries;

public sealed class RoundedRectangleGeometry : Geometry
{

    public RoundedRectangleGeometry(RoundedRectangle roundedRectangle)
    {
        _roundedRectangle = roundedRectangle;
        Figures = CreateFigures(in roundedRectangle);
    }

    private protected override FigureBatch Figures { get; }

    private static FigureBatch CreateFigures(in RoundedRectangle roundedRectangle)
    {
        // TODO: We did't verify the shape.
        var radius = new Vector2(roundedRectangle.RadiusX, roundedRectangle.RadiusY);
        var rect = roundedRectangle.Rectangle;

        var sink = new SimplifiedGeometrySink();

        sink.BeginFigure(new Vector2(rect.Right, (rect.Top + rect.Bottom) / 2), FigureBegin.Filled);
        {
            sink.AddLine(new Vector2(rect.Right, rect.Bottom - radius.Y));
            sink.InternalAddArc(new ArcSegment
            {
                ArcSize = ArcSize.Small,
                Point = new Vector2(rect.Right - radius.X, rect.Bottom),
                RotationAngle = 0,
                Size = radius,
                SweepDirection = SweepDirection.Clockwise,
            });
            sink.AddLine(new Vector2(rect.Left + radius.X, rect.Bottom));
            sink.InternalAddArc(new ArcSegment
            {
                ArcSize = ArcSize.Small,
                Point = new Vector2(rect.Left, rect.Bottom - radius.Y),
                RotationAngle = 0,
                Size = radius,
                SweepDirection = SweepDirection.Clockwise,
            });
            sink.AddLine(new Vector2(rect.Left, rect.Top + radius.Y));
            sink.InternalAddArc(new ArcSegment
            {
                ArcSize = ArcSize.Small,
                Point = new Vector2(rect.Left + radius.X, rect.Top),
                RotationAngle = 0,
                Size = radius,
                SweepDirection = SweepDirection.Clockwise,
            });
            sink.AddLine(new Vector2(rect.Right - radius.X, rect.Top));
            sink.InternalAddArc(new ArcSegment
            {
                ArcSize = ArcSize.Small,
                Point = new Vector2(rect.Right, rect.Top + radius.Y),
                RotationAngle = 0,
                Size = radius,
                SweepDirection = SweepDirection.Clockwise,
            });
        }
        sink.EndFigure(FigureEnd.Closed); // ... so we don't need to manually add the final line (to the origin).

        sink.Close();

        return sink.GetFigureBatch();
    }

    private readonly RoundedRectangle _roundedRectangle;

}
