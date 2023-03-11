using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace MonoGame.Extended;

/// <inheritdoc />
/// <summary>
/// The base class that implements Disposable pattern.
/// This class must be inherited.
/// </summary>
public abstract class DisposableBase : IDisposable
{

    ~DisposableBase()
    {
        if (IsDisposed)
        {
            return;
        }

        IsDisposed = true;

        Dispose(false);
        Disposed?.Invoke(this, EventArgs.Empty);
    }

    [PublicAPI]
    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        Dispose(true);

        IsDisposed = true;

        Disposed?.Invoke(this, EventArgs.Empty);

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Gets whether this object is already disposed.
    /// </summary>
    [PublicAPI]
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Raised when this object is disposed.
    /// </summary>
    [PublicAPI]
    public event EventHandler<EventArgs>? Disposed;

    /// <summary>
    /// Processes disposing logic. This method must be overridden.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> if it is called by a <see cref="Dispose"/> call, <see langword="false"/> if it is called by a finalizer.</param>
    [PublicAPI]
    protected abstract void Dispose(bool disposing);

    /// <summary>
    /// Ensures that this object is not disposed. Otherwise an <see cref="ObjectDisposedException"/> will be thrown.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when current object is disposed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [PublicAPI]
    protected void EnsureNotDisposed()
    {
        if (IsDisposed)
        {
            throw new ObjectDisposedException("this");
        }
    }

}
