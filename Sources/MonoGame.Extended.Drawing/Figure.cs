using Microsoft.Xna.Framework;

namespace MonoGame.Extended.Drawing;

internal sealed class Figure
{

    public Figure(Vector2 origin, GeometryElement[] elements, FigureBegin figureBegin, FigureEnd figureEnd)
    {
        Origin = origin;
        Elements = elements;
        FigureBegin = figureBegin;
        FigureEnd = figureEnd;
    }

    public Vector2 Origin { get; }

    public GeometryElement[] Elements { get; }

    public FigureBegin FigureBegin { get; }

    public FigureEnd FigureEnd { get; }

}
