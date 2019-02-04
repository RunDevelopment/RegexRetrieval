﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace RegexRetrieval.Matcher
{
    public class PositionalSubStringTrieMatcher
    {
        private static readonly Selection<int>[] ListWithEmptySelection = new  [] { Selection<int>.Empty };

        private readonly TrieNode[] roots;

        public CreationOptions Options { get; }


        public PositionalSubStringTrieMatcher(string[] words, CreationOptions options, bool leftToRight)
        {
            Options = options;
            roots = CreatePositionalTrees(words, options, leftToRight);
        }


        #region creation

        private static TrieNode[] CreatePositionalTrees(string[] words, CreationOptions options, bool leftToRight)
        {
            var maxWordLength = words.Select(w => w.Length).Max();

            var selection = new int[words.Length].Set(i => i);
            var rootNodes = new TrieNode[maxWordLength].Set(i =>
            {
                Console.Write("{0}/{1}\r", i, maxWordLength);

                // create root
                var r = TrieNode.CreateRoot(words, selection);

                // add sub nodes
                AddSubNodes(r, i, options, leftToRight);

                // to save memory
                r.ForgetSelection();
                r.ForgetWords();

                return r;
            });

            return rootNodes;
        }

        private static void AddSubNodes(TrieNode node, int position, CreationOptions options, bool leftToRight, List<int>[] lists = null)
        {
            if (!node.IsRoot)
            {
                if (node.Path.Length >= options.MaxDepth) return;
                if (node.SelectionLength < options.MinSplit) return;
            }

            if (lists == null)
                lists = new List<int>[0x10000].Set(i => new List<int>());
            var words = node.RootWords;

            foreach (int s in node.Selection)
            {
                var word = words[s];
                if (position < word.Length)
                    lists[leftToRight ? word[position] : word[word.Length - 1 - position]].Add(s);
            }

            node.AddAsSubNodes(lists, true);

            foreach (var n in node.SubNodes)
                AddSubNodes(n, position + 1, options, leftToRight, lists);
        }

        #endregion

        #region get

        /// <summary>
        /// Returns the node that matches the longest prefix of the given string. If a prefix of the given string 
        /// cannot match any word, null will be returned.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private TrieNode GetNode(int position, SubString value)
        {
            if (position < 0 || position + value.Length > roots.Length) return null;

            var node = roots[position];
            foreach (var c in value)
            {
                if (node.IsLeaf) return node;
                if (!node.TryGetSubNode(c, out node)) return null;
            }
            return node;
        }

        /// <summary>
        /// Returns the minimal number of selection which intersection will only match all words with the given string
        /// at the given position. If no selection could be made, an empty list will be returned.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public ICollection<Selection<int>> GetSelections(int position, SubString value)
        {
            if (Options.MaxDepth <= 0) return Array.Empty< Selection<int>>();
            if (position < 0 || position + value.Length > roots.Length) return ListWithEmptySelection;

            var nodes = new List<TrieNode>(value.Length / Options.MaxDepth + 2);

            /*
             * Separate the string into segments where each segment will correspond to a node.
             * 
             * i.g. with MaxSubStringLength = 3:    mes | sag | e
             */

            // TODO: This splitting method is not optimal when we only use the minimal selection

            int i = 0;
            while (i < value.Length)
            {
                var node = GetNode(position + i, value.ToSubString(i));
                if (node == null) return ListWithEmptySelection;
                nodes.Add(node);
                i += node.Path.Length;
            }

            /*
             * If the last segment is smaller than MaxSubStringLength, its selection might be greater than necessary.
             * (see the example above)
             * This can be solved by adding character from the previous segment.
             */

            if (nodes.Count > 1 && !nodes[nodes.Count - 1].IsLeaf)
            {
                var lastNode = nodes[nodes.Count - 1];
                i = value.Length - lastNode.Path.Length;
                while (--i >= 0)
                {
                    var n = GetNode(position + i, value.ToSubString(i));
                    if (i + n.Path.Length < value.Length)
                        break;
                    lastNode = n;
                }
                nodes[nodes.Count - 1] = lastNode;
            }

            // sort by ascending selection size
            nodes.Sort((a, b) => a.SelectionLength - b.SelectionLength);

            return nodes.Select(n => new Selection<int>( n.Selection)).ToList();
        }

        #endregion

        public class CreationOptions
        {
            public int MaxDepth { get; set; } = 2;
            public int MinSplit { get; set; } = 1000;
        }

    }
}