using System;
using System.Text.RegularExpressions;

namespace RegexRetrieval.Queries
{
    public static class QueryPredicate
    {
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
                tokens[query.Tokens.Count - 1].TokenType == QueryToken.Type.Words)
            {
                // because we always match the whole word, the matching direction doesn't matter for correctness,
                // BUT the regex engine can better optimize for sub strings like this and we can avoid backtracking.
                // Example: /^[\s\S]*bb$/
                options |= RegexOptions.RightToLeft;

            }

            var r = new Regex("^(?:" + query.ToRegexPattern() + ")$", options);

            var min = query.MinLength;
            var max = query.MaxLength;

            return str =>
            {
                return str.Length >= min && str.Length <= max && r.IsMatch(str);
            };
        }

        private static Func<string, bool> TryStringOperations(Query query)
        {
            // the following only work for placeholders and words
            if (query.CharSetTokenCount != 0 || query.OptionalTokenCount != 0) return null;
            // regex is more efficient for constant word. (yeah, I know...)
            if (query.WordTokenCount == 1 && query.Tokens.Count == 1) return null;

            // it really hard to detect and optimize for multiple words
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

            // regex is faster than we can be when it comes to QMarks and words
            if (query.StarTokenCount == 0) return null;

            // From here on, we have exactly one word token and at least one Star token

            // no QMarks
            if (query.QMarkTokenCount == 0 && query.StarTokenCount > 0)
            {
                // There are only 3 possible configurations

                // 1: *abc* == contains("abc")
                if (tokens.Count == 3)
                {
                    var word = tokens[1].Value;
                    return str => str.Contains(word, StringComparison.Ordinal);
                }
                // 2: *abc == endsWith("abc")
                else if (tokens[0].TokenType == QueryToken.Type.Star)
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

            // From here on, we have exactly one word token, at least one Star token, and at least one QMark token

            // TODO: Expand this to queries like ??*abc* , *abc??* , and ??*abc??*

            return null;
        }
    }
}
