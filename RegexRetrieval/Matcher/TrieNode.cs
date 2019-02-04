using System;
using System.Collections.Generic;

namespace RegexRetrieval.Matcher
{
    internal class TrieNode
    {

        #region field

        public readonly char Character;
        public readonly string Path;
        public readonly TrieNode Parent;
        private readonly Dictionary<char, TrieNode> subNodes = new Dictionary<char, TrieNode>();

        private string[] words;
        private int[] selection;
        private readonly int selectionLength;

        #endregion

        #region properties

        public bool IsRoot => Parent == null;
        public bool IsLeaf => subNodes.Count == 0;
        public IEnumerable<TrieNode> SubNodes => subNodes.Values;
        /// <summary>
        /// The number of characters encoded by this node.
        /// </summary>
        public int Depth => Path.Length;

        public string[] RootWords => words;

        public int[] Selection => selection;
        public int SelectionLength => selectionLength;

        #endregion

        #region constructors

        private TrieNode(char c, TrieNode parent, int[] selection)
        {
            Character = c;
            Path = parent.Path + c.ToString();
            Parent = parent;
            words = parent.words;
            this.selection = selection;
            selectionLength = selection?.Length ?? 0;
        }
        private TrieNode(string[] words, int[] selection)
        {
            Character = '\0';
            Path = "";
            Parent = null;
            this.words = words;
            this.selection = selection;
            selectionLength = selection?.Length ?? words?.Length ?? 0;
        }

        #endregion

        public void AddSubNode(char c, int[] selection)
            => subNodes.Add(c, new TrieNode(c, this, selection));
        public bool TryGetSubNode(char c, out TrieNode node)
            => subNodes.TryGetValue(c, out node);

        internal void ForgetWords()
        {
            words = null;
            foreach (var n in subNodes.Values)
                n.ForgetWords();
        }
        internal void ForgetSelection()
        {
            selection = null;
        }

        public static TrieNode CreateRoot(string[] words, int[] selection = null)
            => new TrieNode(words, selection);

        public static IEnumerable<TrieNode> FilterTransitive(IEnumerable<TrieNode> nodes, bool isSorted = false)
        {
            if (!isSorted)
            {
                var sorted = new List<TrieNode>(nodes);
                sorted.Sort((a, b) => b.Path.Length - a.Path.Length);
                nodes = sorted;
            }

            var prefixTree = new SubStringTree();
            foreach (var node in nodes)
            {
                var path = node.Path;
                if (!prefixTree.Contains(path))
                {
                    yield return node;
                    prefixTree.Add(path);
                }
            }
        }

    }
}
