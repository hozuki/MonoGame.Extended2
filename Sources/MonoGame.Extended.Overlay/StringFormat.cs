namespace MonoGame.Extended.Overlay;

public sealed class StringFormat
{

    public StringFormat()
    {
        Align = TextAlign.Left;
        VerticalAlign = VerticalTextAlign.Top;
    }

    public bool IsVertical { get; set; }

    public TextAlign Align { get; set; }

    public VerticalTextAlign VerticalAlign { get; set; }

    public float? PreferredLineHeight { get; set; }

}
