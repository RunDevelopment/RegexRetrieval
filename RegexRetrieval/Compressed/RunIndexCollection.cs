using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RegexRetrieval.Compressed
{
    public sealed class RunIndexCollection : IReadOnlyCollection<int>
    {
        private readonly SimpleSpan<int>[] spans;

        public int Count { get; }


        /// <summary>
        /// Creates a new index array from a sorted (asc) set of indexes.
        /// </summary>
        /// <param name="collection"></param>
        public RunIndexCollection(IEnumerable<int> sortedIndexes)
        {
            (Count, spans) = CreateSpans(sortedIndexes);
        }

        private static (int count, SimpleSpan<int>[] spans) CreateSpans(IEnumerable<int> sortedIndexes)
        {
            var _spans = sortedIndexes.AsSpans().ToArray();
            return ((int) _spans.SpanLengthCount(), _spans);
        }


        public IEnumerator<int> GetEnumerator()
        {
            foreach (var span in spans)
            {
                int i = span.Index;

                var end = span.Index + span.Length - 1;
                while (i != end)
                    yield return i++;
                yield return end;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
