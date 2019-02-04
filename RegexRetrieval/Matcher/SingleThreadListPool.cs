using System;
using System.Collections.Generic;

namespace RegexRetrieval.Matcher
{
    internal class SingleThreadListPool<T> : IDisposable
    {
        private readonly Stack<List<T>> available = new Stack<List<T>>();

        public List<T> Lend()
        {
            if (available.Count > 0)
                return available.Pop();
            else
                return new List<T>();
        }

        public void Return(List<T> list)
        {
            list.Clear();
            available.Push(list);
        }

        public void Dispose()
        {
            while (available.Count > 0)
                available.Pop().Clear();
        }
    }
    internal class SingleThreadListPool : IDisposable
    {
        private readonly Dictionary<Type, object> pools = new Dictionary<Type, object>();

        private SingleThreadListPool<T> GetPool<T>()
        {
            if (pools.TryGetValue(typeof(T), out var pool))
            {
                return (SingleThreadListPool<T>) pool;
            }
            else
            {
                var newPool = new SingleThreadListPool<T>();
                pools.Add(typeof(T), newPool);
                return newPool;
            }
        }

        public List<T> Lend<T>() => GetPool<T>().Lend();

        public void Return<T>(List<T> list) => GetPool<T>().Return(list);

        public void Dispose()
        {
            foreach (var pool in pools.Values)
            {
                (pool as IDisposable)?.Dispose();
            }
            pools.Clear();
        }
    }
}
