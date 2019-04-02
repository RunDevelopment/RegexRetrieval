using System;

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
            var index = new int[bitMask + 1][];

            for (int i = 0; i < words.Length; i++)
            {
                ref var item = ref index[Hash(words[i]) & bitMask];
                if (item == null)
                {
                    item = new[] { i };
                }
                else
                {
                    var newArray = new int[item.Length + 1];
                    Array.Copy(item, newArray, item.Length);
                    newArray[newArray.Length - 1] = i;
                    item = newArray;
                }
            }

            return index;
        }

        private static int Hash(string str) => str.GetHashCode();

        public int GetIndex(string word)
        {
            var hash = Hash(word);
            var wordIndex = index[hash & bitMask];
            if (wordIndex == null) return -1;

            foreach (var i in wordIndex)
            {
                if (word.Equals(words[i], StringComparison.Ordinal))
                    return i;
            }
            return -1;
        }

    }
}
