using System;
using System.Collections;
using System.Collections.Generic;

namespace RegexRetrieval
{
    internal sealed class SortedIndexCollection : IReadOnlyCollection<int>
    {
        private readonly Span[] spans;

        public int Count { get; }


        /// <summary>
        /// Creates a new index array from a sorted (asc) set of indexes.
        /// </summary>
        /// <param name="collection"></param>
        public SortedIndexCollection(IEnumerable<int> sortedIndexes)
        {
            (Count, spans) = CreateSpans(sortedIndexes);
        }

        private static (int count, Span[] spans) CreateSpans(IEnumerable<int> sortedIndexes)
        {
            long _count = 0;
            var _spans = new List<Span>(1024);

            int last = 0;
            int length = 0;
            bool noLast = true;

            foreach (var i in sortedIndexes)
            {
                if (i < 0) throw new InvalidOperationException(
                    $"{nameof(SortedIndexCollection)} does not allow negative indexes.");

                _count++;

                if (noLast)
                {
                    last = i;
                    length = 1;
                    noLast = false;
                }
                else if (i == last + length)
                {
                    length++;
                }
                else
                {
                    if (length > 0)
                    {
                        _spans.Add(new Span(last, length));
                    }
                    last = i;
                    length = 1;
                }
            }

            if (length > 0)
            {
                _spans.Add(new Span(last, length));
            }

            if (_count > int.MaxValue) _count = int.MaxValue;

            return ((int) _count, _spans.ToArray());
        }


        public IEnumerator<int> GetEnumerator()
        {
            foreach (var span in spans)
            {
                var end = span.Start + span.Length;
                for (int i = span.Start; i < end; i++)
                    yield return i;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private readonly struct Span
        {
            public readonly int Start;
            public readonly int Length;

            public Span(int start, int length)
            {
                Start = start;
                Length = length;
            }

            public override string ToString()
                => $"{nameof(Span)}(start={Start}, length={Length})";
        }
    }
}
