using System.Collections.Generic;

namespace RegexRetrieval
{
    public readonly struct Selection<T>
    {
        public IEnumerable<T> Value { get; }
        public int Count { get; }

        public bool IsEmpty => Count == 0;
        public bool IsAll => Value == null && Count != 0;

        public Selection(IEnumerable<T> value, int count)
        {
            Value = value;
            Count = count;
        }
        public Selection(IReadOnlyCollection<T> value) : this(value, value?.Count ?? 0) { }

        public static Selection<T> Empty { get; } = new Selection<T>(null, 0);
        public static Selection<T> All { get; } = new Selection<T>(null, int.MaxValue);
    }
}
