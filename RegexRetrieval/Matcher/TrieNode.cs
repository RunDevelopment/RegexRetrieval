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

        #endregion

        #region properties

        public bool IsRoot => Parent == null;
        public bool IsLeaf => subNodes.Count == 0;
        public IEnumerable<TrieNode> SubNodes => subNodes.Values;
        /// <summary>
        /// The number of characters encoded by this node.
        /// </summary>
        public int Depth => Path.Length;

        public Selection<int> Selection { get; }

        #endregion

        #region constructors

        private TrieNode(char c, TrieNode parent, Selection<int> selection)
        {
            Character = c;
            Parent = parent;
            Selection = selection;
            
            Path = parent == null ? "" : parent.Path + c.ToString();
        }

        #endregion

        public void AddSubNode(char c, Selection<int> selection)
            => subNodes.Add(c, new TrieNode(c, this, selection));
        public bool TryGetSubNode(char c, out TrieNode node)
            => subNodes.TryGetValue(c, out node);

        public static TrieNode CreateRoot(Selection<int> selection)
            => new TrieNode('\0', null, selection);

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
