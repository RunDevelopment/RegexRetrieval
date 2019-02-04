using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RegexRetrieval.Queries
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// For easier processing, this uses an internal query syntax which is a superset of the Netspeak syntax:
    /// <para>
    /// The following are special characters and not allow as characters: <c>?*+()[]{}|</c>
    /// </para>
    /// <para>
    /// <list type="bullet">
    /// <item>
    /// <c>?</c> matches any one character. Regex: /[\s\S]/
    /// </item>
    /// <item>
    /// <c>*</c> matches any number characters. Regex: /[\s\S]*/
    /// </item>
    /// <item>
    /// <c>[abc]</c> matches one characters that is in the set. Regex: /[abc]/
    /// </item>
    /// <item>
    /// <c>{foo}</c> matches the word zero or one times. Regex: /(?:foo)?/
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    public class Query
    {
        public List<QueryToken> Tokens { get; }

        /// <summary>
        /// The minimum number of characters a word has to have to match the query.
        /// </summary>
        public int MinLength { get; }
        /// <summary>
        /// The maximum number of characters a word can have to match the query.
        /// </summary>
        public int MaxLength { get; }
        /// <summary>
        /// Whether the query can match only words with finitely many characters.
        /// </summary>
        public bool FiniteLength => MaxLength != int.MaxValue;

        /// <summary>
        /// The number of words the query can potentially match. This value is always greater than zero.
        /// <para>
        /// If the number of potential matches is (effectively) infinite, this will be <see cref="int.MaxValue"/>.
        /// </para>
        /// </summary>
        public int Combinations { get; }
        /// <summary>
        /// Whether the query can match only finitely many words.
        /// </summary>
        public bool FiniteWords => Combinations < int.MaxValue;

        /// <summary>
        /// Whether the query will match any word.
        /// </summary>
        public bool MatchAny => (WordTokenCount | QMarkTokenCount | CharSetTokenCount) == 0 &&
            StarTokenCount > 0;
        /// <summary>
        /// Whether the query only contains placeholders.
        /// </summary>
        public bool PlaceholdersOnly => (WordTokenCount | CharSetTokenCount | OptionalTokenCount) == 0;

        public int WordTokenCount { get; }
        public int QMarkTokenCount { get; }
        public int StarTokenCount { get; }
        public int CharSetTokenCount { get; }
        public int OptionalTokenCount { get; }

        public Query(List<QueryToken> tokens)
        {
            if (tokens.Count == 0) throw new ArgumentException("There has to be at least one token", nameof(tokens));

            Tokens = tokens;

            var min = 0;
            var max = 0;
            var comb = 1;

            var tokensWord = 0;
            var tokensQMark= 0;
            var tokensStar = 0;
            var tokensCharSet = 0;
            var tokensOptional = 0;

            foreach (var token in tokens)
            {
                switch (token.TokenType)
                {
                    case QueryToken.Type.Words:
                        tokensWord++;
                        min += token.Value.Length;
                        max += token.Value.Length;
                        break;
                    case QueryToken.Type.QMark:
                        tokensQMark++;
                        min++;
                        max++;
                        comb = int.MaxValue;
                        break;
                    case QueryToken.Type.Star:
                        tokensStar++;
                        max = int.MinValue;
                        comb = int.MaxValue;
                        break;
                    case QueryToken.Type.CharSet:
                        tokensCharSet++;
                        min++;
                        max++;
                        comb = ClampMult(comb, token.Value.Length);
                        break;
                    case QueryToken.Type.Optional:
                        tokensOptional++;
                        max += token.Value.Length;
                        comb = ClampMult(comb, 2);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            if (max < 0) max = int.MaxValue;

            MinLength = min;
            MaxLength = max;
            Combinations = comb;

            WordTokenCount = tokensWord;
            QMarkTokenCount = tokensQMark;
            StarTokenCount = tokensStar;
            CharSetTokenCount = tokensCharSet;
            OptionalTokenCount = tokensOptional;
        }
        private static int ClampMult(int a, int b)
            => (int) Math.Max(int.MinValue, Math.Min(Math.BigMul(a, b), int.MaxValue));

        public IEnumerable<string> GetCombinations()
        {
            if (!FiniteWords) throw new InvalidOperationException();

            if (Combinations == 1) return new[] { Tokens[0].Value /* adjacent words are combined */ };

            var words = new List<StringBuilder>(Combinations)
            {
                new StringBuilder()
            };

            void AddToAll(IList<string> suffixes)
            {
                var first = suffixes[0];
                var count = words.Count;

                for (int j = 1; j < suffixes.Count; j++)
                {
                    var suf = suffixes[j];
                    for (int i = 0; i < count; i++)
                        words.Add(new StringBuilder(words[i].ToString()).Append(suf));
                }

                if (first.Length > 0)
                    for (int i = 0; i < count; i++)
                        words[i].Append(first);
            }

            var array1 = new string[1];
            var array2 = new string[2] { "", null };
            var list = new List<string>();

            foreach (var token in Tokens)
            {
                switch (token.TokenType)
                {
                    case QueryToken.Type.Words:
                        array1[0] = token.Value;
                        AddToAll(array1);
                        break;
                    case QueryToken.Type.CharSet:
                        foreach (var c in token.Value)
                            list.Add(c.ToString());
                        AddToAll(list);
                        list.Clear();
                        break;
                    case QueryToken.Type.Optional:
                        array2[1] = token.Value;
                        AddToAll(array2);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            return words.Select(w => w.ToString());
        }

        public string ToRegexPattern()
        {
            var sb = new StringBuilder();

            foreach (var token in Tokens)
            {
                switch (token.TokenType)
                {
                    case QueryToken.Type.Words:
                        sb.Append(Regex.Escape(token.Value));
                        break;
                    case QueryToken.Type.QMark:
                        sb.Append(@"[\s\S]");
                        break;
                    case QueryToken.Type.Star:
                        sb.Append(@"[\s\S]*");
                        break;
                    case QueryToken.Type.CharSet:
                        sb.Append('[').Append(Regex.Escape(token.Value)).Append(']');
                        break;
                    case QueryToken.Type.Optional:
                        sb.Append('[').Append(Regex.Escape(token.Value)).Append(']').Append('?');
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            return sb.ToString();
        }

        public IEnumerable<PositionalSubString> GetSubStrings(int maxPosition = 100_000)
        {
            // get the ranges of each sub string from left to right
            List<(string Str, int Start, int Stop)> GetRanges(List<QueryToken> tokens)
            {
                var res = new List<(string, int start, int stop)>(4);

                int start = 0, stop = 0;

                foreach (var token in tokens)
                {
                    switch (token.TokenType)
                    {
                        case QueryToken.Type.Words:
                            var value = token.Value;
                            res.Add((value, Math.Min(start, maxPosition), Math.Min(stop, maxPosition)));
                            start += value.Length;
                            stop += value.Length;
                            break;
                        case QueryToken.Type.QMark:
                        case QueryToken.Type.CharSet:
                            start++;
                            stop++;
                            break;
                        case QueryToken.Type.Star:
                            stop = maxPosition;
                            break;
                        case QueryToken.Type.Optional:
                            stop++;
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }

                return res;
            }

            // get ranges left to right and right to left
            var ltr = GetRanges(Tokens);
            var rtl = GetRanges(Tokens.ReverseTokens());

            // combine to PositionedSubString
            for (int i = 0; i < ltr.Count; i++)
            {
                var l = ltr[i];
                var r = rtl[rtl.Count - i - 1];
                yield return new PositionalSubString(l.Str.ToSubString(), l.Start, l.Stop, r.Start, r.Stop);
            }
        }
    }

}
