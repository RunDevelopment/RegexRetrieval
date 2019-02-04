using System.Collections;
using System.Collections.Generic;

namespace RegexRetrieval
{
    public class NewSortedIndexCollection : IReadOnlyCollection<int>
    {
        /*
         * The data format:
         * 
         * Is the left most bit (128) of the current byte set?
         *  no)  Offset-mode:
         *       The following v-int + 1 gives the offset from the current index to the next.
         *  yes) Run-mode:
         *       The following v-int + 1 gives the number of incrementing indexes.
         */
        public readonly byte[] data;
        private readonly long count;

        public int Count => count >= int.MaxValue ? int.MaxValue : (int) count;

        public NewSortedIndexCollection(IEnumerable<int> indexCollection)
        {
            (data, count) = CreateData(indexCollection);
        }


        private static (byte[] bytes, long count) CreateData(IEnumerable<int> collection)
        {
            var bytes = new List<byte>(64);

            int last = 0;
            int runLength = 1;

            byte[] vintBuffer = new byte[5]; // 5 bytes are enough to vint encode 32 bits

            // TODO: Doesn't work

            long count = 0;
            foreach (var index in collection)
            {
                count++;

                if (index == last + runLength)
                {
                    // increment run length
                    runLength++;
                }
                else
                {
                    bool firstBit = runLength != 1;
                    int n;

                    if (firstBit)
                    {
                        // output run
                        n = runLength - 1;
                        runLength = 1;
                    }
                    else
                    {
                        // output offset
                        n = index - last - 1;
                    }

                    WriteVInt(bytes, vintBuffer, n, firstBit);
                }
            }

            if (runLength != 1)
            {
                // output run
                WriteVInt(bytes, vintBuffer, runLength - 1, true);
            }

            return (bytes.ToArray(), count);
        }

        private static void WriteVInt(List<byte> bytes, byte[] buffer, int n, bool firstBit)
        {
#if DEBUG
            if (n < 0) throw new ArgumentOutOfRangeException(nameof(n));
#endif

            if (n <= 63)
            {
                if (firstBit) n |= 128; // set first bit
                bytes.Add((byte) n);
                return;
            }

            int bytesUsed = 0;
            while (n != 0)
            {
                buffer[bytesUsed++] = (byte) (n & 127);
                n >>= 7;
            }

            // insert a zero if the first part needs 7 bits
            if (buffer[bytesUsed - 1] > 63)
                buffer[bytesUsed++] = 0;

            byte b = 64;
            if (firstBit) b = 128 | 64;

            bytes.Add((byte) (b | buffer[--bytesUsed]));
            for (int i = bytesUsed - 1; i > 0; i--)
                bytes.Add((byte) (128 | buffer[i]));
            bytes.Add(buffer[0]);
        }

        private IEnumerable<int> Enumerate()
        {
            int lastIndex = 0;
            for (int i = 0; i < data.Length; i++)
            {
                byte b = data[i];

                // the v-int
                int n = b & 127;
                if (n > 63) // some more bytes follow
                {
                    i++;
                    n = ReadVInt(data, ref i, n & 63);
                }

                if (b > 127)
                {
                    // run mode
                    int end = lastIndex + n + 1;
                    for (; lastIndex <= end; lastIndex++)
                        yield return lastIndex;
                    lastIndex = end;
                }
                else
                {
                    // offset mode
                    yield return lastIndex += 1 + n;
                }
            }
        }

        private static int ReadVInt(byte[] bytes, ref int index, int start)
        {
            byte b;
            while ((b = bytes[index]) > 127)
            {
                start = (start << 7) | (b & 127);
                index++;
            }
            start = (start << 7) | b;
            return start;
        }

        public IEnumerator<int> GetEnumerator()
            => Enumerate().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
