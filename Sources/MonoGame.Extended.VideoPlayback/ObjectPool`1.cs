using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace MonoGame.Extended.VideoPlayback;

/// <inheritdoc />
/// <summary>
/// An object pool implementation.
/// </summary>
/// <typeparam name="T">Type of objects in the pool.</typeparam>
internal sealed class ObjectPool<T> : DisposableBase
{

    /// <summary>
    /// Creates a new <see cref="ObjectPool{T}"/> instance.
    /// </summary>
    /// <param name="collectThreshold">Threshold when calling <see cref="Collect"/>.</param>
    /// <param name="alloc">Allocation function.</param>
    /// <param name="dealloc">Deallocation function.</param>
    internal ObjectPool(int collectThreshold, Func<T> alloc, Action<T> dealloc)
        : this(collectThreshold, alloc, dealloc, null)
    {
    }

    /// <summary>
    /// Creates a new <see cref="ObjectPool{T}"/> instance.
    /// </summary>
    /// <param name="collectThreshold">Threshold when calling <see cref="Collect"/>.</param>
    /// <param name="alloc">Allocation function.</param>
    /// <param name="dealloc">Deallocation function.</param>
    /// <param name="reset">Object reset function.</param>
    internal ObjectPool(int collectThreshold, Func<T> alloc, Action<T> dealloc, RefAction<T>? reset)
    {
        _objectsInUse = new HashSet<T>();
        _freeObjects = new LinkedList<T>();
        _collectThreshold = collectThreshold;
        _alloc = alloc;
        _dealloc = dealloc;
        _reset = reset;
    }

    /// <summary>
    /// Acquires an object. If there is no object available, it will call <see cref="Alloc"/> to create one.
    /// </summary>
    /// <returns>Acquired or allocated object.</returns>
    internal T Acquire()
    {
        EnsureNotDisposed();

        if (NumberOfObjectsInUse == Count)
        {
            return Alloc();
        }

        if (_freeObjects.Count == 0)
        {
            throw new ApplicationException("This should not happen");
        }

        // Move the object from free list to using list
        var firstNode = _freeObjects.First;
        _freeObjects.RemoveFirst();

        var obj = firstNode!.Value;
        _objectsInUse.Add(obj);

        return obj;
    }

    /// <summary>
    /// Allocates a new object and returns it.
    /// </summary>
    /// <returns>Allocated object.</returns>
    internal T Alloc()
    {
        EnsureNotDisposed();

        var alloc = _alloc;

        Debug.Assert(alloc != null, nameof(alloc) + " != null");

        var obj = alloc();

        _objectsInUse.Add(obj);

        return obj;
    }

    /// <summary>
    /// Releases an object.
    /// </summary>
    /// <param name="obj">The object to release. If this value is <see langword="null"/>, the method does nothing.</param>
    internal bool Release(T? obj)
    {
        return Release(obj, false);
    }

    /// <summary>
    /// Releases and deallocates an object.
    /// </summary>
    /// <param name="obj">The object to destroy. If this value is <see langword="null"/>, the method does nothing.</param>
    /// <returns><see langword="true"/> if the object is found in the pool and successfully destroyed, otherwise <see langword="false"/>.</returns>
    internal bool Destroy(T? obj)
    {
        return Release(obj, true);
    }

    /// <summary>
    /// Deallocates all unused objects.
    /// If number of objects in this pool is smaller than the threshold set when creating the pool, this method does nothing.
    /// </summary>
    internal void Collect()
    {
        EnsureNotDisposed();

        var dealloc = _dealloc;

        Debug.Assert(dealloc != null, nameof(dealloc) + " != null");

        if (NumberOfObjectsInUse <= _collectThreshold)
        {
            // No need to collect.
            return;
        }

        if (_freeObjects.Count == 0)
        {
            // Nothing to collect.
            return;
        }

        // Create a temporary storage and perform deallocation on it.
        var freeObjects = new List<T>(_freeObjects);
        _freeObjects.Clear();

        var objects = freeObjects.ToArray();

        foreach (var obj in objects)
        {
            dealloc(obj);
        }
    }

    /// <summary>
    /// Destroys all objects in this pool. Used for preparing restarting playback.
    /// </summary>
    internal void Reset()
    {
        var dealloc = _dealloc;

        Debug.Assert(dealloc != null, nameof(dealloc) + " != null");

        foreach (var obj in _objectsInUse)
        {
            dealloc(obj);
        }

        foreach (var obj in _freeObjects)
        {
            dealloc(obj);
        }

        _objectsInUse.Clear();
        _freeObjects.Clear();
    }

    /// <summary>
    /// Number of objects in this pool.
    /// </summary>
    internal int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        get => _objectsInUse.Count + _freeObjects.Count;
    }

    /// <summary>
    /// Number of objects in use.
    /// </summary>
    internal int NumberOfObjectsInUse
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        get => _objectsInUse.Count;
    }

    protected override void Dispose(bool disposing)
    {
        Reset();

        _alloc = null;
        _dealloc = null;
        _reset = null;
    }

    private bool Release(T? obj, bool deallocObject)
    {
        EnsureNotDisposed();

        if (ReferenceEquals(obj, null))
        {
            return false;
        }

        if (!_objectsInUse.Contains(obj))
        {
            return false;
        }

        if (deallocObject)
        {
            // Remove the object from using list
            _objectsInUse.Remove(obj);

            var dealloc = _dealloc;

            Debug.Assert(dealloc != null, nameof(dealloc) + " != null");

            dealloc(obj);
        }
        else
        {
            // Move the object from using list to free list
            _objectsInUse.Remove(obj);

            _reset?.Invoke(ref obj);

            _freeObjects.AddLast(obj);
        }

        return true;
    }

    [ItemNotNull]
    private readonly HashSet<T> _objectsInUse;

    [ItemNotNull]
    private readonly LinkedList<T> _freeObjects;

    private readonly int _collectThreshold;

    private Func<T>? _alloc;

    private Action<T>? _dealloc;

    private RefAction<T>? _reset;

}
