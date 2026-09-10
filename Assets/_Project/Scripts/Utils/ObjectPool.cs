using System;
using System.Collections.Generic;

namespace OrbitDash.Utils
{
    /// <summary>
    /// Minimal generic object pool. Reuses instances instead of repeated Instantiate/Destroy calls,
    /// which matters for mobile performance once obstacles spawn continuously (see ObstacleSpawner).
    /// </summary>
    public class ObjectPool<T>
    {
        private readonly Stack<T> _inactive = new();
        private readonly Func<T> _createInstance;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;

        /// <param name="createInstance">Creates a brand new instance. Called only when the pool is empty.</param>
        /// <param name="onGet">Called right before an instance is handed out (e.g. re-enable it).</param>
        /// <param name="onRelease">Called right after an instance is returned (e.g. disable it, reset state).</param>
        /// <param name="prewarmCount">Number of instances to create and pool immediately.</param>
        public ObjectPool(Func<T> createInstance, Action<T> onGet = null, Action<T> onRelease = null, int prewarmCount = 0)
        {
            _createInstance = createInstance ?? throw new ArgumentNullException(nameof(createInstance));
            _onGet = onGet;
            _onRelease = onRelease;

            for (int i = 0; i < prewarmCount; i++)
            {
                T instance = _createInstance();
                _onRelease?.Invoke(instance);
                _inactive.Push(instance);
            }
        }

        public int InactiveCount => _inactive.Count;

        public T Get()
        {
            T instance = _inactive.Count > 0 ? _inactive.Pop() : _createInstance();
            _onGet?.Invoke(instance);
            return instance;
        }

        public void Release(T instance)
        {
            _onRelease?.Invoke(instance);
            _inactive.Push(instance);
        }
    }
}
