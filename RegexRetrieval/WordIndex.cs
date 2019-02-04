using System;
using System.Collections.Generic;

namespace RegexRetrieval
{
    public class WordIndex
    {
        // TODO: variable length
        private const int LengthBitMask = 0xFFFFF;

        private readonly string[] words;
        private readonly int[][] index;

        public WordIndex(string[] words)
        {
            this.words = words;
            this.index = CreateIndex(words);
        }

        private static int[][] CreateIndex(string[] words)
        {
            var expectedLength = words.Length / (LengthBitMask + 1);
            expectedLength += expectedLength >> 1;
            var lists = new List<int>[LengthBitMask + 1].Set(i => new List<int>(expectedLength));

            for (int i = 0; i < words.Length; i++)
            {
                int hash = Hash(words[i]);
                lists[hash & LengthBitMask].Add(i);
            }

            var index = new int[lists.Length][];
            for (int i = 0; i < index.Length; i++)
            {
                var binaryIndex = lists[i].ToArray();
                lists[i] = null;
                Array.Sort(binaryIndex, (a, b) => String.CompareOrdinal(words[a], words[b]));
                if (binaryIndex.Length > 0)
                    index[i] = binaryIndex;
            }

            return index;
        }

        private static int Hash(string str)
        {
            int i = str.Length * 4093;
            foreach (var c in str)
            {
                i += c;
                i += i << 10;
                i ^= i >> 6;
            }
            i ^= i << 3;
            i ^= i >> 11;
            i ^= i << 15;
            return i;
        }

        private int BinarySearch(int[] binaryIndex, string word)
        {
            int low = 0;
            int high = binaryIndex.Length;

            while (low < high)
            {
                var middle = low + ((high - low) >> 1);
                var c = String.CompareOrdinal(word, words[binaryIndex[middle]]);
                if (c == 0) return binaryIndex[middle];
                if (c > 0) low = middle + 1;
                else high = middle;
            }
            return -1;
        }

        public int GetIndex(string word)
        {
            var hash = Hash(word);
            var binaryIndex = index[hash & LengthBitMask];
            if (binaryIndex == null) return -1;
            return BinarySearch(binaryIndex, word);
        }

    }
}
