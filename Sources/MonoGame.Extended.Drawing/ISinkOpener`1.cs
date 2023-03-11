using System;

namespace MonoGame.Extended.Drawing;

internal interface ISinkOpener<TSink>
    where TSink : class, ISink
{

    TSink OpenImpl()
    {
        if (ActiveSink != null)
        {
            throw new InvalidOperationException();
        }

        var sink = CreateSink();

        ActiveSink = sink;

        return sink;
    }

    void CloseImpl(TSink sink)
    {
        if (ActiveSink != sink)
        {
            throw new ArgumentException("Closing sink does not match with active sink.", nameof(sink));
        }

        OnSinkClosed(sink);

        ActiveSink = null;
    }

    protected TSink CreateSink();

    protected void OnSinkClosed(TSink sink);

    protected TSink? ActiveSink { get; set; }

}
