using System;
using JetBrains.Annotations;

namespace MonoGame.Extended.Drawing.Geometries;

[PublicAPI]
public sealed class PathGeometry : Geometry, ISinkOpener<GeometrySink>
{

    public PathGeometry()
        : this(false)
    {
    }

    internal PathGeometry(bool isTextGeometry)
    {
        IsTextGeometry = isTextGeometry;
        _figureBatch = new FigureBatch(Array.Empty<Figure>(), SimplifiedGeometrySink.DefaultFillMode);
    }

    public GeometrySink Open()
    {
        return ((ISinkOpener<GeometrySink>)this).OpenImpl();
    }

    internal void Close(GeometrySink sink)
    {
        ((ISinkOpener<GeometrySink>)this).CloseImpl(sink);
    }

    internal bool IsTextGeometry { get; }

    private protected override FigureBatch Figures => _figureBatch;

    GeometrySink? ISinkOpener<GeometrySink>.ActiveSink { get; set; }

    GeometrySink ISinkOpener<GeometrySink>.CreateSink()
    {
        return new GeometrySink(this);
    }

    void ISinkOpener<GeometrySink>.OnSinkClosed(GeometrySink sink)
    {
        _figureBatch = sink.GetFigureBatch();
        InvalidateTesselationResultCache();
    }

    private FigureBatch _figureBatch;

}
