using System;
using System.Collections.Concurrent;

namespace RegexRetrieval
{
    public sealed class ObjectPool<T>
        where T : class
    {
        private readonly ConcurrentStack<T> available = new ConcurrentStack<T>();

        public Func<T> Supplier { get; }
        public Action<T> Cleanup { get; }
        public Action<T> ClearAction { get; }

        private static readonly Action<T> NoOp = o => { };

        public ObjectPool(Func<T> supplier) : this(supplier, NoOp) { }
        public ObjectPool(Func<T> supplier, Action<T> cleanup) : this(supplier, cleanup, NoOp) { }
        public ObjectPool(Func<T> supplier, Action<T> cleanup, Action<T> clearAction)
        {
            Supplier = supplier ?? throw new ArgumentNullException(nameof(supplier));
            Cleanup = cleanup ?? throw new ArgumentNullException(nameof(cleanup));
            ClearAction = clearAction ?? throw new ArgumentNullException(nameof(clearAction));
        }

        public T Lend() => Lend(Supplier);
        public T Lend(Func<T> supplier)
        {
            if (supplier == null) throw new ArgumentNullException(nameof(supplier));

            if (available.TryPop(out var obj))
                return obj;
            return supplier();
        }

        public void Return(T obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            Cleanup(obj);
            available.Push(obj);
        }

        public void Clear()
        {
            var buffer = new T[16];
            int count;
            while ((count = available.TryPopRange(buffer)) != 0)
            {
                for (int i = 0; i < count; i++)
                {
                    ClearAction(buffer[i]);
                }
            }
        }
    }
}
