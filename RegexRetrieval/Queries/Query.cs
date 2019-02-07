using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RegexRetrieval.Queries
{
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
            var tokensQMark = 0;
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

        public IEnumerable<string> GetCombinations(int maxWordLength = 256)
        {
            if (!FiniteWords) throw new InvalidOperationException();

            var tokens = Tokens;

            if (Combinations == 1) return new[] { tokens[0].Value /* adjacent words are combined */ };

            char[] chars = new char[maxWordLength];
            IEnumerable<string> YieldStrings(int tokenIndex, int startIndex)
            {
                if (tokenIndex >= tokens.Count)
                {
                    yield return new string(chars, 0, startIndex);
                }
                else
                {
                    var token = tokens[tokenIndex];
                    switch (token.TokenType)
                    {
                        case QueryToken.Type.Words:
                            {
                                if (startIndex + token.Value.Length > maxWordLength)
                                    yield break;

                                // add all characters
                                var i = startIndex;
                                foreach (var c in token.Value)
                                    chars[i++] = c;

                                // yield return all combinations
                                foreach (var str in YieldStrings(tokenIndex + 1, startIndex + i))
                                    yield return str;
                                break;
                            }
                        case QueryToken.Type.CharSet:
                            {
                                if (startIndex + 1 > maxWordLength)
                                    yield break;

                                foreach (var c in token.Value)
                                {
                                    chars[startIndex] = c;

                                    // yield return all combinations
                                    foreach (var str in YieldStrings(tokenIndex + 1, startIndex + 1))
                                        yield return str;
                                }
                                break;
                            }
                        case QueryToken.Type.Optional:
                            {
                                // yield return all combinations WITHOUT the optional
                                foreach (var str in YieldStrings(tokenIndex + 1, startIndex))
                                    yield return str;

                                if (startIndex + token.Value.Length > maxWordLength)
                                    yield break;

                                // add all characters
                                var i = startIndex;
                                foreach (var c in token.Value)
                                    chars[i++] = c;

                                // yield return all combinations WITH the optional
                                foreach (var str in YieldStrings(tokenIndex + 1, startIndex + i))
                                    yield return str;
                                break;
                            }
                        default:
                            throw new InvalidOperationException();
                    }
                }
            }

            return YieldStrings(0, 0);
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
