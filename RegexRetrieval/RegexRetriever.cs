#define INFO

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using RegexRetrieval.Matcher;
using RegexRetrieval.Queries;

namespace RegexRetrieval
{
    public class RegexRetriever : IRegexRetriever
#if (INFO)
        , IInfoProvider
#endif
    {

        /// <summary>
        /// If the number of potential matches of the query is less than this value then all potential matches will
        /// be generate and indexed via a <see cref="WordIndex"/>.
        /// <para>
        /// This won't have any effect if no word index is available.
        /// </para>
        /// </summary>
        private const int WordIndexOptimizationThreshold = 100;
        /// <summary>
        /// The maximum relative size difference between the smallest the selection and the current one for it to be
        /// a part of the minimal selection.
        /// </summary>
        private const double MaxSelectionSizeDifferenceThreshold = 25.0;

        public readonly string[] Words;

        private readonly LengthMatcher lengthMatcher;
        private readonly SubStringTrieMatcher subStringTrie;
        private readonly PositionalSubStringTrieMatcher ltrPositionalTrie;
        private readonly PositionalSubStringTrieMatcher rtlPositionalTrie;
        private readonly WordIndex wordIndex;

        #region info

        private const string InfoOptimizationMethod = "Optimization";

#if (INFO)

        IReadOnlyList<InfoMeta> IInfoProvider.InfoMetadata { get; } = new InfoMeta[]
        {
            new InfoMeta(name: InfoOptimizationMethod, length: 20),
        };

        private readonly Dictionary<string, object> infos = new Dictionary<string, object>();

        bool IInfoProvider.TryGetInfo(string name, out object value)
            => infos.TryGetValue(name, out value);

#endif

        private void ResetInfo()
        {
#if (INFO)
            infos.Clear();
#endif
        }
        private void SetInfo(string name, object value)
        {
#if (INFO)
            infos[name] = value;
#endif
        }

        #endregion


        public RegexRetriever(string[] words, CreationOptions options)
        {
            Words = words;

            options.SubStringTrieOptions = options.SubStringTrieOptions ?? new SubStringTrieMatcher.CreationOptions();
            options.LTRPositionSubStringTrieOptions = options.LTRPositionSubStringTrieOptions ?? new PositionalSubStringTrieMatcher.CreationOptions();
            options.RTLPositionSubStringTrieOptions = options.RTLPositionSubStringTrieOptions ?? new PositionalSubStringTrieMatcher.CreationOptions();

            var logger = options.Logger ?? (s => { });

            T Create<T>(string name, Func<T> supplier)
            {
                logger($"Creating {name}...");
                var watch = Stopwatch.StartNew();

                var res = supplier();

                logger($"\rCreated {name} ({watch.ElapsedTicks * 1.0 / Stopwatch.Frequency:0.#}s)");

                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                GC.Collect();

                return res;
            }

            if (options.UseWordIndex)
            {
                wordIndex = Create(nameof(WordIndex), () => new WordIndex(words));
            }

            if (options.UseLTRPositionSubStringTrie)
            {
                ltrPositionalTrie = Create($"LTR {nameof(PositionalSubStringTrieMatcher)}",
                    () => new PositionalSubStringTrieMatcher(words, options.LTRPositionSubStringTrieOptions, true));
            }

            if (options.UseRTLPositionSubStringTrie)
            {
                rtlPositionalTrie = Create($"RTL {nameof(PositionalSubStringTrieMatcher)}",
                    () => new PositionalSubStringTrieMatcher(words, options.RTLPositionSubStringTrieOptions, false));
            }

            if (options.UseSubStringTrie)
            {
                subStringTrie = Create(nameof(SubStringTrieMatcher),
                    () => new SubStringTrieMatcher(words, options.SubStringTrieOptions));
            }

            if (options.UseLengthMatcher)
            {
                lengthMatcher = Create(nameof(LengthMatcher), () => new LengthMatcher(words));
            }
        }


        public IEnumerable<string> Retrieve(Query query, int count)
        {
            ResetInfo();

            IEnumerable<string> IndexWords(IEnumerable<int> selectionIndexes = null)
            {
                if (selectionIndexes == null)
                    return Words.Take(count);
                else
                    return Words.Index(selectionIndexes).Take(count);
            }


            // "*" matches all words
            if (query.MatchAny)
            {
                SetInfo(InfoOptimizationMethod, "MatchAny");

                return IndexWords();
            }

            // check all potential words using a word index
            if (TryFiniteCombinationsOptimization(query, out var indexes))
                return IndexWords(indexes);

            // with only placeholders we don't even need to try sub strings
            if (TryPlaceholderOnlyOptimization(query, out indexes))
                return IndexWords(indexes);


            // Various selections
            var selections = new List<Selection<int>>();

            // sub strings
            var subStringRanges = query.GetSubStrings().ToList();
            var subStrTree = new SubStringTree();
            foreach (var range in subStringRanges)
            {
                if (ltrPositionalTrie != null && range.LeftFixed)
                {
                    // ltr
                    selections.AddRange(ltrPositionalTrie.GetSelections(range.Left, range.Value));
                    subStrTree.Add(range.Value);
                }
                else if (rtlPositionalTrie != null && range.RightFixed)
                {
                    // rtl
                    selections.AddRange(rtlPositionalTrie.GetSelections(range.Right, range.Value.ToString().Reverse().ToSubString()));
                    subStrTree.Add(range.Value);
                }

                // TODO: When both LTR and RTL are available and the range is fixed for both left and right, we have a choice
            }
            // simple sub strings
            // Note that this is the last resort. Substring tries have no chance to compete with positional tries.
            if (subStringTrie != null)
            {
                selections.AddRange(subStringTrie.GetSelection(subStringRanges.Select(r => r.Value), subStrTree));
            }


            // length
            if (lengthMatcher != null)
            {
                var lengthSelection = lengthMatcher.GetSelection(query.MinLength, query.MaxLength);

                if (!lengthSelection.IsAll)
                    selections.Add(lengthSelection);
            }

            /*
             * There might be quite a number of selections but we are only interested in the best selections.
             */
            selections.Sort((a, b) => a.Count.CompareTo(b.Count));


            // minimal selection
            IEnumerable<int> minSelection = null;

            if (selections.Count != 0)
            {
                var first = selections[0];

                if (first.IsEmpty)
                {
                    // we found an empty selection
                    return Array.Empty<string>();
                }

                minSelection = first.Value;
            }

            if (selections.Count >= 2)
            {
                var first = selections[0];
                var second = selections[1];

                // it's important to intersect 2 at most because after that the intersection overhead takes over
                if (second.Count < first.Count * MaxSelectionSizeDifferenceThreshold)
                {
                    minSelection = minSelection.SetIntersect(second.Value);
                }
            }


            // match
            var predicate = query.CreatePredicate();

            if (minSelection == null) return Words.Where(predicate).Take(count);
            else return Words.Index(minSelection).Where(predicate).Take(count);
        }

        private bool TryFiniteCombinationsOptimization(Query query, out IEnumerable<int> indexes)
        {
            // give out empty selection
            indexes = Array.Empty<int>();

            if (wordIndex == null || !query.FiniteWords || query.Combinations > WordIndexOptimizationThreshold)
                return false;

            // for logging
            SetInfo(InfoOptimizationMethod, "WordIndex");

            // use word index
            var _indexes = new List<int>(query.Combinations);
            foreach (var word in query.GetCombinations())
            {
                var i = wordIndex.GetIndex(word);
                if (i != -1) _indexes.Add(i);
            }

            // we give out empty by default
            if (_indexes.Count == 0) return true;

            // sort remaining to filter out (possible) duplicates
            _indexes.Sort();
            indexes = _indexes.Unique();

            return true;
        }

        private bool TryPlaceholderOnlyOptimization(Query query, out IEnumerable<int> indexes)
        {
            indexes = Array.Empty<int>();

            if (lengthMatcher == null || !query.PlaceholdersOnly)
                return false;

            SetInfo(InfoOptimizationMethod, "PlaceholdersOnly");

            var sel = lengthMatcher.GetSelection(query.MinLength, query.MaxLength);
            if (!sel.IsEmpty) indexes = sel.Value;

            return true;
        }

        public class CreationOptions
        {
            public bool UseWordIndex { get; set; } = true;

            public bool UseLengthMatcher { get; set; } = true;

            public bool UseSubStringTrie { get; set; } = true;
            public SubStringTrieMatcher.CreationOptions SubStringTrieOptions { get; set; } = null;

            public bool UseLTRPositionSubStringTrie { get; set; } = true;
            public PositionalSubStringTrieMatcher.CreationOptions LTRPositionSubStringTrieOptions { get; set; } = null;

            public bool UseRTLPositionSubStringTrie { get; set; } = true;
            public PositionalSubStringTrieMatcher.CreationOptions RTLPositionSubStringTrieOptions { get; set; } = null;

            public Action<string> Logger { get; set; } = Console.WriteLine;

            public CreationOptions() { }
            public CreationOptions(bool initializeWithDefaultOptions)
            {
                SubStringTrieOptions = new SubStringTrieMatcher.CreationOptions();
                LTRPositionSubStringTrieOptions = new PositionalSubStringTrieMatcher.CreationOptions();
                RTLPositionSubStringTrieOptions = new PositionalSubStringTrieMatcher.CreationOptions();
            }
        }
    }
}
