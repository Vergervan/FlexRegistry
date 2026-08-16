using System;
using System.Collections.Generic;
using System.Linq;

namespace FlexRegistry.Utils
{
    public class AhoCorasick
    {
        private readonly TrieNode _root = new TrieNode();
        private bool _isBuilt;

        public AhoCorasick(IEnumerable<string> words)
        {
            foreach (var word in words)
                AddWord(word);
        }

        private void AddWord(string word)
        {
            var node = _root;
            foreach (var c in word)
            {
                if (!node.Children.TryGetValue(c, out var next))
                {
                    next = new TrieNode();
                    node.Children[c] = next;
                }
                node = next;
            }
            node.Word = word;
        }

        public void Build()
        {
            var queue = new Queue<TrieNode>();
            _root.Fail = _root;

            foreach (var child in _root.Children.Values)
            {
                child.Fail = _root;
                queue.Enqueue(child);
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                foreach (var child in current.Children)
                {
                    var failNode = current.Fail;

                    while (failNode != _root && !failNode.Children.ContainsKey(child.Key))
                        failNode = failNode.Fail;

                    if (failNode.Children.TryGetValue(child.Key, out var failChild))
                        child.Value.Fail = failChild;
                    else
                        child.Value.Fail = _root;

                    queue.Enqueue(child.Value);
                }
            }
            _isBuilt = true;
        }

        public List<(string word, int index)> FindAll(string text)
        {
            if (!_isBuilt) Build();

            var results = new List<(string, int)>();
            var current = _root;

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];

                while (current != _root && !current.Children.ContainsKey(c))
                    current = current.Fail;

                if (current.Children.TryGetValue(c, out var next))
                    current = next;
                else
                    current = _root;

                var temp = current;
                while (temp != _root)
                {
                    if (temp.Word != null)
                        results.Add((temp.Word, i - temp.Word.Length + 1));

                    temp = temp.Fail;
                }
            }
            return results;
        }

        private class TrieNode
        {
            public Dictionary<char, TrieNode> Children { get; } = new Dictionary<char, TrieNode>();
            public TrieNode Fail { get; set; }
            public string Word { get; set; }
        }
    }
}
