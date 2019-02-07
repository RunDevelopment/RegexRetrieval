using System;
using System.Collections.Generic;
using System.Linq;

namespace RegexRetrieval.Matcher
{
    public class SubStringTrieMatcher
    {
        private static readonly Selection<int>[] ListWithEmptySelection = new[] { Selection<int>.Empty };

        private readonly TrieNode root;
        public CreationOptions Options { get; }


        public SubStringTrieMatcher(string[] words, int maxDepth)
            : this(words, new CreationOptions() { MaxDepth = maxDepth }) { }
        public SubStringTrieMatcher(string[] words, CreationOptions options)
        {
            Options = options;
            root = CreateTree(words, options);
        }


        #region creation

        private static TrieNode CreateTree(string[] words, CreationOptions options)
        {
            var rootNode = TrieNode.CreateRoot(words);

            var listsPool = MatcherUtil.CreateListArrayPool(words.GetCharactersDistribution());

            AddSubNodes(rootNode, options, listsPool);

            listsPool.Clear();

            return rootNode;
        }
        private static void AddSubNodes(TrieNode node, CreationOptions options, ObjectPool<List<int>[]> listsPool)
        {
            if (node.Path.Length >= options.MaxDepth) return;
            if (node.SelectionLength < options.MinSplit) return;

            var lists = listsPool.Lend();
            var charCounter = new int[0x10000].Set(i => -1);

            string[] words = node.RootWords;
            string stack = node.Path;

            if (node.IsRoot)
            {
                // optimization because we know that: node.Selection == null && stack == ""
                for (int s = 0; s < node.SelectionLength; s++)
                {
                    var word = words[s];

                    foreach (var c in word)
                    {
                        // so that we don't add a word twice
                        if (charCounter[c] == s) continue;
                        charCounter[c] = s;

                        lists[c].Add(s);
                    }
                }
            }
            else
            {
                foreach (int s in node.Selection)
                {
                    var word = words[s];
                    if (word.Length <= stack.Length) continue;

                    int offset = 0;
                    while (offset < word.Length)
                    {
                        int index = word.IndexOf(stack, offset, word.Length - offset - 1, StringComparison.Ordinal);
                        if (index == -1) break;
                        offset = index + 1;

                        // get next char
                        var c = word[index + stack.Length];

                        // so that we don't add a word twice
                        if (charCounter[c] == s) continue;
                        charCounter[c] = s;

                        lists[c].Add(s);
                    }
                }
            }


            node.AddAsSubNodes(lists);
            listsPool.Return(lists);

            if (node.IsRoot)
            {
                node.SubNodes.AsParallel().ForAll(n =>
                {
                    AddSubNodes(n, options, listsPool);
                });
            }
            else
            {
                foreach (var n in node.SubNodes)
                {
                    AddSubNodes(n, options, listsPool);
                }
            }
        }

        #endregion

        #region get

        /// <summary>
        /// Returns the node that matches the longest prefix of the given string. If a prefix of the given string 
        /// cannot match any word, null will be returned.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private TrieNode GetNode(SubString value)
        {
            var node = root;
            foreach (var c in value)
            {
                if (node.IsLeaf) return node;
                if (!node.TryGetSubNode(c, out node)) return null;
            }
            return node;
        }

        /// <summary>
        /// Returns a list of disjoint selection for the given substrings.
        /// The sub strings of the returned selection are guaranteed to not be included by <paramref name="processedSubStrings"/>.
        /// <para>
        /// This will modify <paramref name="processedSubStrings"/>, adding all substrings used for the returned selections.
        /// </para>
        /// </summary>
        /// <param name="subStrings"></param>
        /// <param name="processedSubStrings"></param>
        /// <returns></returns>
        public ICollection<Selection<int>> GetSelection(IEnumerable<SubString> subStrings, SubStringTree processedSubStrings)
        {
            if (root.IsLeaf) return Array.Empty<Selection<int>>();

            var selections = new List<Selection<int>>(4);
            var foundUnknownSubString = false;

            // Note that this operates in O(n*log(n)) where n is the number of characters in subStr
            void AddSubString(SubString subStr)
            {
                if (foundUnknownSubString) return;
                if (subStr.Length == 0) return; // nothing to do here
                if (processedSubStrings.Contains(subStr.ToString())) return; // already int selection

                // get the node with the minimal selection length of all possible nodes for this substring
                TrieNode minNode = null;
                int minIndex = -1;
                for (int i = 0; i < subStr.Length; i++)
                {
                    var node = GetNode(subStr.ToSubString(i));
                    if (node == null)
                    {
                        // we can abort immediately once we found a substring which is not included in the list of 
                        // words used to create the trie
                        foundUnknownSubString = true;
                        return;
                    }

                    if (!processedSubStrings.Contains(node.Path) && // don't include substrings we already have
                        (minNode == null || node.SelectionLength < minNode.SelectionLength))
                    {
                        minNode = node;
                        minIndex = i;
                    }

                    // we don't need to check the last small substrings as they are already included by the first node
                    // which reaches to the end of the substring
                    if (i + node.Depth == subStr.Length)
                        break;
                }

                if (minNode == null) return; // the substring did not include any now substrings

                // add the minimal selection to the other selections
                selections.Add(new Selection<int>(minNode.Selection));
                processedSubStrings.Add(minNode.Path);

                // and recursively get the selection of the remaining parts of the substring
                AddSubString(subStr.ToSubString(0, minIndex));
                AddSubString(subStr.ToSubString(minIndex + minNode.Depth));
            }

            foreach (var item in subStrings)
                AddSubString(item);

            if (foundUnknownSubString) return ListWithEmptySelection;

            return selections;
        }

        #endregion


        public class CreationOptions
        {
            public int MaxDepth { get; set; } = 2;
            public int MinSplit { get; set; } = 1000;
        }

    }
}
