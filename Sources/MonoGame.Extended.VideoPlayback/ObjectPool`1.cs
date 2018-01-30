using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace MonoGame.Extended.VideoPlayback {
    /// <inheritdoc />
    /// <summary>
    /// An object pool implementation.
    /// </summary>
    /// <typeparam name="T">Type of objects in the pool.</typeparam>
    internal sealed class ObjectPool<T> : DisposableBase {

        /// <summary>
        /// Creates a new <see cref="ObjectPool{T}"/> instance.
        /// </summary>
        /// <param name="collectThreshold">Threshold when calling <see cref="Collect"/>.</param>
        /// <param name="alloc">Allocation function.</param>
        /// <param name="dealloc">Deallocation function.</param>
        internal ObjectPool(int collectThreshold, [NotNull] Func<T> alloc, [NotNull] Action<T> dealloc) {
            _allocatedObjects = new Dictionary<T, bool>();
            _collectThreshold = collectThreshold;
            _alloc = alloc;
            _dealloc = dealloc;
        }

        /// <summary>
        /// Acquires an object. If there is no object available, it will call <see cref="Alloc"/> to create one.
        /// </summary>
        /// <returns>Acquired or allocated object.</returns>
        internal T Acquire() {
            EnsureNotDisposed();

            if (ObjectsInUse == Count) {
                return Alloc();
            }

            foreach (var kv in _allocatedObjects) {
                if (!kv.Value) {
                    _allocatedObjects[kv.Key] = true;
                    ++_objectsInUse;

                    return kv.Key;
                }
            }

            throw new ApplicationException("This should not have happened.");
        }

        /// <summary>
        /// Allocates a new object and returns it.
        /// </summary>
        /// <returns>Allocated object.</returns>
        internal T Alloc() {
            EnsureNotDisposed();

            Debug.Assert(_alloc != null, nameof(_alloc) + " != null");

            var obj = _alloc();

            _allocatedObjects[obj] = true;
            ++_objectsInUse;

            return obj;
        }

        /// <summary>
        /// Releases an object.
        /// </summary>
        /// <param name="obj">The object to release. If this value is <see langword="null"/>, the method does nothing.</param>
        internal void Release([CanBeNull] T obj) {
            EnsureNotDisposed();

            if (obj == null) {
                return;
            }

            if (!_allocatedObjects.ContainsKey(obj)) {
                throw new KeyNotFoundException();
            }

            _allocatedObjects[obj] = false;
            --_objectsInUse;
        }

        /// <summary>
        /// Releases and deallocates an object.
        /// </summary>
        /// <param name="obj">The object to destroy. If this value is <see langword="null"/>, the method does nothing.</param>
        /// <returns><see langword="true"/> if the object is found in the pool and successfully destroyed, otherwise <see langword="false"/>.</returns>
        internal bool Destroy([CanBeNull] T obj) {
            EnsureNotDisposed();

            if (obj == null) {
                return false;
            }

            Debug.Assert(_dealloc != null, nameof(_dealloc) + " != null");

            if (!_allocatedObjects.ContainsKey(obj)) {
                return false;
            }

            _allocatedObjects.Remove(obj);
            --_objectsInUse;
            _dealloc(obj);

            return true;
        }

        /// <summary>
        /// Deallocates all unused objects.
        /// If number of objects in this pool is smaller than the threshold set when creating the pool, this method does nothing.
        /// </summary>
        internal void Collect() {
            EnsureNotDisposed();

            var dealloc = _dealloc;

            Debug.Assert(dealloc != null, nameof(dealloc) + " != null");

            if (_allocatedObjects.Count <= _collectThreshold) {
                return;
            }

            var freeObjects = new List<T>();

            foreach (var kv in _allocatedObjects) {
                if (!kv.Value) {
                    freeObjects.Add(kv.Key);
                }
            }

            if (freeObjects.Count > 0) {
                var arr = freeObjects.ToArray();

                for (var i = 0; i < arr.Length; ++i) {
                    var obj = arr[i];

                    dealloc(obj);

                    _allocatedObjects.Remove(arr[i]);
                }

                _objectsInUse -= arr.Length;
            }
        }

        /// <summary>
        /// Destroys all objects in this pool. Used for preparing restarting playback.
        /// </summary>
        internal void Reset() {
            var dealloc = _dealloc;

            Debug.Assert(dealloc != null, nameof(dealloc) + " != null");

            foreach (var kv in _allocatedObjects) {
                var obj = kv.Key;

                dealloc(obj);
            }

            _allocatedObjects.Clear();
            _objectsInUse = 0;
        }

        /// <summary>
        /// Number of objects in this pool.
        /// </summary>
        internal int Count => _allocatedObjects.Count;

        /// <summary>
        /// Number of objects in use.
        /// </summary>
        internal int ObjectsInUse => _objectsInUse;

        protected override void Dispose(bool disposing) {
            Reset();

            _alloc = null;
            _dealloc = null;
        }

        private readonly Dictionary<T, bool> _allocatedObjects;
        private readonly int _collectThreshold;

        private int _objectsInUse;

        [CanBeNull]
        private Func<T> _alloc;
        [CanBeNull]
        private Action<T> _dealloc;

    }
}
