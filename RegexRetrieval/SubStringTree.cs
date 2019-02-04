using System.Collections.Generic;

namespace RegexRetrieval
{
    public class SubStringTree
    {
        private readonly Node root = new Node('\0');

        public void Add(IEnumerable<char> word)
        {
            var nodes = new List<Node>() { root };
            foreach (var c in word)
            {
                for (var i = 0; i < nodes.Count; i++)
                {
                    if (nodes[i].nodes.TryGetValue(c, out var next))
                        nodes[i] = next;
                    else
                    {
                        var n = new Node(c);
                        nodes[i].nodes.Add(c, n);
                        nodes[i] = n;
                    }
                }
                nodes.Add(root); 
            }
        }

        public bool Contains(IEnumerable<char> prefix)
        {
            var n = root;
            foreach (var c in prefix)
            {
                if (n.nodes.TryGetValue(c, out var next))
                    n = next;
                else return false;
            }
            return true;
        }

        public void Clear()
        {
            root.Clear();
        }


        private class Node
        {
            public readonly char c;
            public readonly Dictionary<char, Node> nodes = new Dictionary<char, Node>();

            public Node(char c)
            {
                this.c = c;
            }

            public void Clear()
            {
                foreach (var n in nodes.Values)
                    n.Clear();
                nodes.Clear();
            }
        }
    }
}
