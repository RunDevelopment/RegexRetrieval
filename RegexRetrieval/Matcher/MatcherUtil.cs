using System.Collections.Generic;

namespace RegexRetrieval.Matcher
{
    internal static class MatcherUtil
    {

        public static void AddAsSubNodes(this TrieNode node, List<int>[] lists, bool reuse = false)
        {
            for (var i = 0; i < lists.Length; i++)
            {
                var l = lists[i];
                if (!reuse) lists[i] = null;
                if (l.Count > 0)
                {
                    node.AddSubNode((char) i, l.ToArray());
                    if (reuse) l.Clear();
                }
            }
        }

    }
}
