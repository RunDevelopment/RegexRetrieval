using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RegexRetrieval.Queries
{
    public static class QueryUtil
    {
        public static readonly Regex QMark = RegExp(@"\?");
        public static readonly Regex Star = RegExp(@"\*");
        public static readonly Regex Plus = RegExp(@"\+");

        public static readonly Regex Curly = RegExp(@"\{([\s\S]*?)\}");
        public static readonly Regex Square = RegExp(@"\[([\s\S]*?)\]");

        private static Regex RegExp(string pattern)
            => new Regex(@"\G(?:" + pattern + ")", RegexOptions.CultureInvariant | RegexOptions.Compiled);


        public static List<QueryToken> ReverseTokens(this IList<QueryToken> tokens)
        {
            var rev = new List<QueryToken>(tokens.Count);

            for (int i = tokens.Count - 1; i >= 0; i--)
            {
                var token = tokens[i];
                switch (token.TokenType)
                {
                    case QueryToken.Type.Words:
                        rev.Add(QueryToken.CreateWord(token.Value.Reverse()));
                        break;
                    case QueryToken.Type.QMark:
                    case QueryToken.Type.Star:
                    case QueryToken.Type.CharSet:
                        rev.Add(token);
                        break;
                    case QueryToken.Type.Optional:
                        rev.Add(QueryToken.CreateOptional(token.Value.Reverse()));
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            return rev;
        }

        #region token stream methods

        public static IList<QueryToken> Optimize(this IList<QueryToken> tokens)
        {
            int delCount = 0;

            HashSet<char> hashSet = null;
            string OptimizeSet(string set)
            {
                if (set.Length <= 1) return set;

                if (hashSet == null)
                    hashSet = new HashSet<char>(set.Length);
                else
                    hashSet.Clear();

                foreach (var c in hashSet)
                    hashSet.Add(c);

                if (hashSet.Count == set.Length) return set;

                var chars = new char[hashSet.Count];
                var i = 0;
                foreach (var c in hashSet)
                    chars[i++] = c;
                return new string(chars);
            }

            int count = tokens.Count;
            for (int i = 1; i < count; i++)
            {
                var token = tokens[i];

                // By incrementing delCount we delete the previous token and replace it with the current one.

                // we can always safely look one element behind
                var prevIndex = i - 1 - delCount;
                var prev = tokens[prevIndex];

                switch (token.TokenType)
                {
                    case QueryToken.Type.Words:
                        if (token.Value.Length == 0)
                        {
                            delCount++;
                        }
                        else if (prev.TokenType == QueryToken.Type.Words)
                        {
                            delCount++;
                            token = QueryToken.CreateWord(prev.Value + token.Value);
                        }
                        break;
                    case QueryToken.Type.QMark:
                        if (prev.TokenType == QueryToken.Type.Star)
                        {
                            // enforce order QMark -> Star
                            tokens[prevIndex] = token;
                            token = prev;
                        }
                        break;
                    case QueryToken.Type.Star:
                        if (prev.TokenType == QueryToken.Type.Star)
                            delCount++;
                        break;
                    case QueryToken.Type.CharSet:
                        var set = OptimizeSet(token.Value);
                        if (set.Length == 1)
                        {
                            if (prev.TokenType == QueryToken.Type.Words)
                            {
                                delCount++;
                                token = QueryToken.CreateWord(prev.Value + set);
                            }
                            else
                            {
                                token = QueryToken.CreateWord(set);
                            }
                        }
                        else if (set.Length < token.Value.Length)
                        {
                            token = QueryToken.CreateCharSet(set);
                        }
                        break;
                    case QueryToken.Type.Optional:
                        if (token.Value.Length == 0)
                            delCount++;
                        break;
                    default:
                        throw new InvalidOperationException();
                }

                tokens[i - delCount] = token;
            }

            if (tokens is List<QueryToken> list)
            {
                list.RemoveRange(tokens.Count - delCount, delCount);
            }
            else
            {
                for (var i = delCount; i > 0; i--)
                    tokens.RemoveAt(tokens.Count - 1);
            }

            return tokens;
        }

        /*
         * Note:
         * All of these methods may alter the token stream in any way.
         * They only guarantee that the resulting query is equal to the one created by tokens.Add(CreateXXX(args)).
         */

        public static IList<QueryToken> AddWord(this IList<QueryToken> tokens, string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value.Length == 0)
                return tokens;

            var count = tokens.Count;
            if (count == 0 || tokens[count - 1].TokenType != QueryToken.Type.Words)
            {
                // new token
                tokens.Add(QueryToken.CreateWord(value));
            }
            else
            {
                // merge with previous word
                tokens[count - 1] = QueryToken.CreateWord(tokens[count - 1].Value + value);
            }
            return tokens;
        }
        public static IList<QueryToken> AddQMark(this IList<QueryToken> tokens)
        {
            var count = tokens.Count;
            if (count > 0 && tokens[count - 1].TokenType == QueryToken.Type.Star)
            {
                var star = tokens[count - 1];
                tokens[count - 1] = QueryToken.CreateQMark();
                tokens.Add(star);
            }
            else
            {
                tokens.Add(QueryToken.CreateQMark());
            }
            return tokens;
        }
        public static IList<QueryToken> AddStar(this IList<QueryToken> tokens)
        {
            var count = tokens.Count;
            if (count == 0 || tokens[count - 1].TokenType != QueryToken.Type.Star)
            {
                tokens.Add(QueryToken.CreateStar());
            }
            return tokens;
        }
        public static IList<QueryToken> AddCharSet(this IList<QueryToken> tokens, string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value.Length == 0)
                throw new ArgumentException("The value of a char set cannot be empty.", nameof(value));

            if (value.Length == 1)
                return tokens.AddWord(value);

            tokens.Add(QueryToken.CreateCharSet(value));

            return tokens;
        }
        public static IList<QueryToken> AddOptional(this IList<QueryToken> tokens, string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value.Length == 0)
                return tokens;

            tokens.Add(QueryToken.CreateOptional(value));

            return tokens;
        }

        #endregion

    }
}
