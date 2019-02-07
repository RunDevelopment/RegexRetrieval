using System.Collections.Generic;

namespace RegexRetrieval.Compressed
{
    internal readonly struct SimpleSpan<T> where T : struct
    {
        public readonly T Index;
        public readonly T Length;

        public SimpleSpan(T index, T length)
        {
            Index = index;
            Length = length;
        }

        public override string ToString()
            => $"{nameof(SimpleSpan<T>)}(index: {Index}, length: {Length})";
    }

    internal static class SimpleSpans
    {
        public static IEnumerable<SimpleSpan<int>> AsSpans(this IEnumerable<int> sortedIndexes)
        {
            int last = 0;
            int length = 1;
            bool noLast = true;

            foreach (var i in sortedIndexes)
            {
                if (noLast)
                {
                    last = i;
                    noLast = false;
                }
                else if (i == last + length)
                {
                    length++;
                }
                else
                {
                    yield return new SimpleSpan<int>(last, length);

                    last = i;
                    length = 1;
                }
            }

            if (length > 0 && !noLast)
            {
                yield return new SimpleSpan<int>(last, length);
            }
        }

        public static long SpanLengthCount(this IEnumerable<SimpleSpan<int>> collection)
        {
            var count = 0;
            foreach (var span in collection)
                count += span.Length;
            return count;
        }
    }
}
