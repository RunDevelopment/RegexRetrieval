using System;
using System.Text;
using System.Text.RegularExpressions;

namespace RegexRetrieval.Queries
{
    public static class QueryPredicate
    {
        internal static string ToRegexPattern(this Query query)
        {
            var sb = new StringBuilder(64);

            foreach (var token in query.Tokens)
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
                        sb.Append("(?:").Append(Regex.Escape(token.Value)).Append(')').Append('?');
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            return sb.ToString();
        }

        public static Func<string, bool> CreatePredicate(this Query query)
        {
            // We assume an optimized token stream.

            // optimized string operations
            var strPredicate = TryStringOperations(query);
            if (strPredicate != null) return strPredicate;

            // Regex it is
            RegexOptions options = RegexOptions.CultureInvariant;

            var tokens = query.Tokens;

            if (tokens[0].TokenType != QueryToken.Type.Words &&
                tokens[tokens.Count - 1].TokenType == QueryToken.Type.Words)
            {
                // because we always match the whole word, the matching direction doesn't matter for correctness,
                // BUT the regex engine can better optimize for sub strings like this and we can avoid backtracking.
                // Example: /^[\s\S]*bb$/
                options |= RegexOptions.RightToLeft;

            }

            var r = new Regex("^(?:" + query.ToRegexPattern() + ")$", options);

            var min = query.MinLength;
            var max = query.MaxLength;

            return str => str.Length >= min && str.Length <= max && r.IsMatch(str);
        }

        private static Func<string, bool> TryStringOperations(Query query)
        {
            // the following only work for placeholders and words
            if (query.CharSetTokenCount != 0 || query.OptionalTokenCount != 0) return null;
            // regex is more efficient for constant word. (yeah, I know...)
            if (query.WordTokenCount == 1 && query.Tokens.Count == 1) return null;

            // it really hard to detect and optimize for multiple words, so just let the regex handle it
            if (query.WordTokenCount > 1) return null;

            var tokens = query.Tokens;

            // empty word only
            if (tokens.Count == 0) return str => str.Length == 0;

            // placeholders only
            if (query.WordTokenCount == 0)
            {
                if (query.MatchAny) return str => true;

                var min = query.MinLength;
                var max = query.MaxLength;

                // either constant length (???)
                if (min == max) return str => str.Length == min;
                // or infinite (??*)
                return str => str.Length > min;
            }

            // Known: Word == 1 && QMark + Star > 0

            // regex is faster than we can be when it comes to QMarks and words (again: yeah, I know...)
            if (query.StarTokenCount == 0) return null;

            // Known: Word == 1 && QMark >= 0 && Star > 0

            // no QMarks && only one star
            if (query.QMarkTokenCount == 0 && query.StarTokenCount == 1)
            {
                // Known: Word == 1 && QMark == 0 && Star == 1

                // There are only 2 possible configurations

                // 2: *abc == endsWith("abc")
                if (tokens[0].TokenType == QueryToken.Type.Star)
                {
                    var word = tokens[1].Value;
                    return str => str.EndsWith(word, StringComparison.Ordinal);
                }
                // 3: abc* == startsWith("abc")
                else
                {
                    var word = tokens[0].Value;
                    return str => str.StartsWith(word, StringComparison.Ordinal);
                }
            }

            // Known: Word == 1 && QMark >= 0 && Star > 0

            var leadingQMarks = 0;
            while (leadingQMarks < tokens.Count && tokens[leadingQMarks].TokenType == QueryToken.Type.QMark)
                leadingQMarks++;
            var trailingQMarks = query.QMarkTokenCount - leadingQMarks;
            var leadingStar = tokens[leadingQMarks].TokenType == QueryToken.Type.Star;
            var trailingStart = tokens[tokens.Count - 1].TokenType == QueryToken.Type.Star;

            {
                var word = tokens[leadingQMarks + (leadingStar ? 1 : 0)].Value;
                var totalQMark = query.QMarkTokenCount;
                var min = query.MinLength;

                // ??*abc??*
                if (leadingStar && trailingStart)
                    return str => str.Length >= min && str.IndexOf(word, leadingQMarks, str.Length - totalQMark, StringComparison.Ordinal) >= 0;

                // ??*abc??
                if (leadingStar)
                    return str => str.Length >= min && str.IndexOf(word, str.Length - word.Length - trailingQMarks, word.Length, StringComparison.Ordinal) >= 0;

                // ??abc??*
                if (trailingStart)
                    return str => str.Length >= min && str.IndexOf(word, leadingQMarks, word.Length, StringComparison.Ordinal) >= 0;
            }

            return null;
        }
    }
}
