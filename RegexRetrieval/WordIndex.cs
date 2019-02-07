using System;
using System.Collections.Generic;

namespace RegexRetrieval
{
    public class WordIndex
    {
        private readonly string[] words;
        private readonly int[][] index;
        private readonly int bitMask;

        public WordIndex(string[] words)
        {
            this.words = words;
            bitMask = NextPowerOfTwo(words.Length) / 2 - 1;
            index = CreateIndex(words, bitMask);
        }

        private static int NextPowerOfTwo(int n)
        {
            n--;
            n |= n >> 1;
            n |= n >> 2;
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;
            n++;
            return n;
        }

        private static int[][] CreateIndex(string[] words, int bitMask)
        {
            var expectedLength = words.Length / (bitMask + 1);
            expectedLength += expectedLength >> 1 + 1;
            var lists = new List<int>[bitMask + 1].Set(i => new List<int>(expectedLength));

            for (int i = 0; i < words.Length; i++)
            {
                int hash = Hash(words[i]);
                lists[hash & bitMask].Add(i);
            }

            var index = new int[lists.Length][];
            for (int i = 0; i < index.Length; i++)
            {
                ref var list = ref lists[i];
                if (list.Count > 0)
                    index[i] = list.ToArray();
                list = null;
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

        public int GetIndex(string word)
        {
            var hash = Hash(word);
            var wordIndex = index[hash & bitMask];
            if (wordIndex == null) return -1;

            foreach (var i in wordIndex)
            {
                var iWord = words[i];
                if (iWord.Equals(word, StringComparison.Ordinal))
                    return i;
            }
            return -1;
        }

    }
}
