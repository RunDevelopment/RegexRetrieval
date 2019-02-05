using System;
using System.Collections.Generic;
using System.Text;

namespace RegexRetrieval.Queries
{
    public static class TokenUtil
    {
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

        public static string UniqueCharSet(string set)
        {
            if (set == null || set.Length < 2) return set;

            var chars = set.ToCharArray();
            Array.Sort(chars, (a, b) => a - b);

            var sb = new StringBuilder(chars.Length);
            char last = '\0';
            bool first = true;
            foreach (var c in chars)
            {
                if (first || last != c)
                {
                    sb.Append(c);
                    last = c;
                    first = false;
                }
            }

            return sb.ToString();
        }

        #region token stream methods

        /*
         * Note:
         * All of these methods may alter the token stream in any way.
         * They only guarantee that the resulting query will match the same words as the one created by 
         * tokens.Add(CreateXXX(args)).
         * 
         * Query equvivalence rules are used to make queries as simple as possible.
         * A query q is simpler that a query p iff 
         *  q.ToString().Length + q.TokenCount < p.ToString().Length + p.TokenCount
         */

        public static IList<QueryToken> AddWord(this IList<QueryToken> tokens, string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            // rule: 1

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
                // rule: 3
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
            // rule: 2
            var count = tokens.Count;
            if (count == 0 || tokens[count - 1].TokenType != QueryToken.Type.Star)
            {
                tokens.Add(QueryToken.CreateStar());

                // rule: 3, 6
                int i = count;
                while (--i >= 0)
                {
                    var token = tokens[i];

                    if (token.TokenType == QueryToken.Type.Optional)
                    {
                        tokens.RemoveAt(i);
                    }
                    else if (token.TokenType != QueryToken.Type.QMark)
                    {
                        break;
                    }
                }
            }
            return tokens;
        }
        public static IList<QueryToken> AddCharSet(this IList<QueryToken> tokens, string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value.Length == 0)
                throw new ArgumentException("The value of a char set cannot be empty.", nameof(value));

            // rule: 4
            if (value.Length == 1)
                return tokens.AddWord(value);

            tokens.Add(QueryToken.CreateCharSet(value));

            return tokens;
        }
        public static IList<QueryToken> AddOptional(this IList<QueryToken> tokens, string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            // rule: 1, 5
            if (value.Length == 0)
                return tokens;

            // rule: 6
            if (tokens.Count != 0 && tokens[tokens.Count - 1].TokenType == QueryToken.Type.Star)
                return tokens;

            tokens.Add(QueryToken.CreateOptional(value));

            return tokens;
        }

        #endregion

    }
}
