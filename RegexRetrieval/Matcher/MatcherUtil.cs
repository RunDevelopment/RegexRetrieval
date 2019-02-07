using System;
using System.Collections.Generic;

namespace RegexRetrieval.Matcher
{
    internal static class MatcherUtil
    {

        public static void AddAsSubNodes(this TrieNode node, List<int>[] lists)
        {
            for (var i = 0; i < lists.Length; i++)
            {
                var l = lists[i];
                if (l != null && l.Count > 0)
                {
                    node.AddSubNode((char) i, l.ToArray());
                }
            }
        }

        public static int[] GetCharactersDistribution(this string[] words)
        {
            var dist = new int[0x1_0000];
            foreach (var w in words)
                foreach (var c in w)
                    dist[c]++;

            return dist;
        }

        public static ObjectPool<List<int>[]> CreateListArrayPool(int[] characterDistribution)
        {
            return new ObjectPool<List<int>[]>(
                supplier: () =>
                {
                    var array = new List<int>[0x10000];
                    for (int i = 0; i < array.Length; i++)
                    {
                        var charCount = characterDistribution[i];
                        array[i] = charCount == 0 ? null : new List<int>(Math.Min(1024, charCount));
                    }
                    return array;
                },
                cleanup: lists =>
                {
                    foreach (var list in lists)
                        list?.Clear();
                },
                clearAction: lists =>
                {
                    for (int i = 0; i < lists.Length; i++)
                        lists[i] = null;
                });
        }

    }
}
