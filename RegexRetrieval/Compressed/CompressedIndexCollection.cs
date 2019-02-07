using System;
using System.Collections;
using System.Collections.Generic;

namespace RegexRetrieval.Compressed
{
    public class CompressedIndexCollection : IReadOnlyCollection<int>
    {
        private readonly ushort[] data;
        private readonly long count;

        public int Count => count >= int.MaxValue ? int.MaxValue : (int) count;

        public CompressedIndexCollection(IEnumerable<int> indexCollection)
        {
            if (indexCollection == null)
                throw new ArgumentNullException(nameof(indexCollection));

            (data, count) = CreateData(indexCollection);
        }


        private static (ushort[] data, long count) CreateData(IEnumerable<int> collection)
        {
            var spans = collection.AsSpans();
            var rangeOrOffsets = CompressedIndexCollectionUtil.AsSimpleIndexRangeOrOffet(spans, -1);

            var list = new List<ushort>(64);
            var tempBuffer = new ushort[5];
            ulong uValue = 0;

            long count = 0;
            foreach (var rangeOrOffset in rangeOrOffsets)
            {
                uint value = rangeOrOffset.RangeLengthOrOffset;

                if (rangeOrOffset.IsRange)
                {
                    count += value;

                    // output run
                    uValue = ((ulong) (value - 1) << 1) | 1; // last bit one
                    VarInt.Write(list, tempBuffer, uValue);
                }
                else
                {
                    count++;

                    // output offset
                    uValue = (ulong) (value - 1) << 1; // last bit zero
                    VarInt.Write(list, tempBuffer, uValue);
                }
            }

            return (list.ToArray(), count);
        }

        private IEnumerable<int> Enumerate()
        {
            int last = -1;
            for (int i = 0; i < data.Length; i++)
            {
                VarInt.Read(data, ref i, out ulong uValue);

                bool isRange = (uValue & 1) != 0; // last bit is set
                int value = (int) (uValue >> 1);

                last++;
                if (isRange)
                {
                    // run mode
                    int end = last + value;
                    for (; last <= end; last++)
                        yield return last;
                    last = end;
                }
                else
                {
                    // offset mode
                    last += value;
                    yield return last;
                }
            }
        }

        public IEnumerator<int> GetEnumerator()
            => Enumerate().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }

    internal readonly struct IndexRangeOrOffset
    {
        public readonly uint RangeLengthOrOffset;
        public readonly bool IsRange;

        public IndexRangeOrOffset(uint rangeLengthOrOffset, bool isRange)
        {
#if DEBUG
            if (rangeLengthOrOffset < 1) throw new ArgumentOutOfRangeException(nameof(rangeLengthOrOffset));
#endif
            RangeLengthOrOffset = rangeLengthOrOffset;
            IsRange = isRange;
        }

        public override string ToString()
        {
            if (IsRange)
            {
                return $"{nameof(IndexRangeOrOffset)}(rangeLength: {RangeLengthOrOffset})";
            }
            else
            {
                return $"{nameof(IndexRangeOrOffset)}(offset: {RangeLengthOrOffset})";
            }
        }
    }

    internal static class CompressedIndexCollectionUtil
    {
        public static IEnumerable<IndexRangeOrOffset> AsSimpleIndexRangeOrOffet(IEnumerable<SimpleSpan<int>> spans, int firstIndexMinusOne = -1)
        {
            int lastIndex = firstIndexMinusOne;

            foreach (var span in spans)
            {
                var index = span.Index;
                var length = span.Length;

                yield return new IndexRangeOrOffset((uint) (index - lastIndex), isRange: false);
                length--;

                if (length > 0)
                {
                    yield return new IndexRangeOrOffset((uint) length, isRange: true);
                }

                lastIndex = index + length;
            }
        }
    }
}
