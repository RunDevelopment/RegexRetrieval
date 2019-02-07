using System;
using System.Collections.Generic;
using System.Linq;
using RegexRetrieval.Compressed;

namespace RegexRetrieval.Matcher
{
    public class LengthMatcher
    {
        private readonly int wordCount;

        private readonly int min;
        private readonly int max;
        /// <summary>
        /// At words selected are exactly index + min characters long.
        /// </summary>
        private readonly CompressedIndexCollection[] exactly;

        /// <summary>
        /// At words selected are at least index + min characters long.
        /// </summary>
        private readonly CompressedIndexCollection[] atLeast;

        public LengthMatcher(string[] words)
        {
            (min, max) = GetMinMaxLength(words);
            wordCount = words.Length;

            var selectionsCount = max - min + 1;

            var pool = new ObjectPool<List<int>>(
                supplier: () => new List<int>(words.Length / 4 + 1),
                cleanup: list => list.Clear());


            exactly = new CompressedIndexCollection[selectionsCount];
            atLeast = new CompressedIndexCollection[selectionsCount];

            void SetExactlyItem(int index)
            {
                int len = index + min;

                var tempList = pool.Lend();
                for (int i = 0; i < words.Length; i++)
                    if (words[i].Length == len)
                        tempList.Add(i);

                exactly[index] = new CompressedIndexCollection(tempList);
                pool.Return(tempList);
            }

            // create at least array
            void SetAtLeastItem(int index)
            {
                int minLen = index + min;

                var tempList = pool.Lend();
                for (int i = 0; i < words.Length; i++)
                    if (words[i].Length >= minLen)
                        tempList.Add(i);

                atLeast[index] = new CompressedIndexCollection(tempList);
                pool.Return(tempList);
            }

            var actions = new List<Action>(selectionsCount * 2);
            for (int i = 0; i < selectionsCount; i++)
            {
                var index = i;
                actions.Add(() => SetExactlyItem(index));
                actions.Add(() => SetAtLeastItem(index));
            }

            actions.AsParallel().WithDegreeOfParallelism(4).ForAll(a => a());
        }

        private static (int Min, int Max) GetMinMaxLength(string[] words)
        {
            int min = int.MaxValue;
            int max = int.MinValue;
            foreach (var w in words)
            {
                if (w.Length < min) min = w.Length;
                if (w.Length > max) max = w.Length;
            }
            return (min, max);
        }

        /// <summary>
        /// Returns a selection of indexes.
        /// The index of a word will be part of the selection if and only if minLength &lt;= word.length &lt;= maxLength.
        /// </summary>
        /// <param name="minLength"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public Selection<int> GetSelection(int minLength, int maxLength)
        {
            if (minLength > maxLength) throw new ArgumentOutOfRangeException(nameof(minLength),
                $"{nameof(minLength)}={minLength} cannot be greater than {nameof(maxLength)}={maxLength}");

            if (minLength == maxLength) return GetExactly(minLength);

            if (minLength > max || maxLength < min) return Selection<int>.Empty;
            // this will match all words -> return null
            if (minLength <= min && maxLength >= max) return Selection<int>.All;

            if (maxLength >= max) return GetAtLeast(minLength);
            if (minLength <= min) return GetAtMost(maxLength);
            return GetBetween(minLength, maxLength);
        }
        private Selection<int> GetExactly(int length)
        {
            var index = length - min;

            // no words of that length
            if (index < 0 || index >= exactly.Length) return Selection<int>.Empty;

            return new Selection<int>(exactly[index]);
        }
        private Selection<int> GetAtLeast(int minLength)
        {
            var index = minLength - min;

            if (index > atLeast.Length) return Selection<int>.Empty;
            if (index < 0) index = 0; // all words
            if (index == 0) return Selection<int>.All;

            return new Selection<int>(atLeast[index]);
        }
        private Selection<int> GetAtMost(int maxLength)
            => GetAtLeast(maxLength + 1).SetComplement(0, wordCount);
        private Selection<int> GetBetween(int minLength, int maxLength)
            => GetAtLeast(minLength).SetWithoutSubSet(GetAtLeast(maxLength + 1));

    }
}
