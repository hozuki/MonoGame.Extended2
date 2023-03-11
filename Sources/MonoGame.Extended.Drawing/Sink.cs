using System;
using JetBrains.Annotations;

namespace MonoGame.Extended.Drawing;

[PublicAPI]
public abstract class Sink : ISink
{

    public void Close()
    {
        EnsureNotClosed();

        _isClosed = true;

        OnClosed();
    }

    protected abstract void OnClosed();

    protected void EnsureNotClosed()
    {
        if (_isClosed)
        {
            throw new InvalidOperationException("The sink is closed.");
        }
    }

    private bool _isClosed;

}
