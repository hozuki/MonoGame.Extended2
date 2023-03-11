namespace MonoGame.Extended.Drawing;

internal sealed class FigureBatch
{

    public FigureBatch(Figure[] figures, FillMode fillMode)
    {
        Figures = figures;
        FillMode = fillMode;
    }

    public Figure[] Figures { get; }

    public FillMode FillMode { get; }

}
