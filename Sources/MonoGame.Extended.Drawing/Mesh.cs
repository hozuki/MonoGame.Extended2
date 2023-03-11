using JetBrains.Annotations;

namespace MonoGame.Extended.Drawing;

[PublicAPI]
public sealed class Mesh : ISinkOpener<TessellationSink>
{

    internal Triangle[]? Triangles { get; private set; }

    public TessellationSink Open()
    {
        return ((ISinkOpener<TessellationSink>)this).OpenImpl();
    }

    internal void Close(TessellationSink sink)
    {
        ((ISinkOpener<TessellationSink>)this).CloseImpl(sink);
    }

    TessellationSink ISinkOpener<TessellationSink>.CreateSink()
    {
        return new TessellationSink(this);
    }

    void ISinkOpener<TessellationSink>.OnSinkClosed(TessellationSink sink)
    {
        Triangles = sink.Triangles;
    }

    TessellationSink? ISinkOpener<TessellationSink>.ActiveSink { get; set; }

}
