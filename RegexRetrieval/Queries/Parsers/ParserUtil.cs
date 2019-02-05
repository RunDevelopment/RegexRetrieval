using System;
using System.Text;
using System.Text.RegularExpressions;

namespace RegexRetrieval.Queries.Parsers
{
    internal static class ParserUtil
    {
        public static Regex ToStickyRegExp(string pattern)
            => new Regex(@"\G(?:" + pattern + ")", RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public static Exception TokenizerError(string msg, string query, int position, int rangeIndex = int.MinValue, int rangeLength = 1)
        {
            if (rangeIndex == int.MinValue) rangeIndex = position;

            var sb = new StringBuilder();
            sb.Append(msg).Append(" at ").Append(position).Append(" in\n");
            sb.Append(query).Append('\n');

            sb.Append(' ', rangeIndex);
            sb.Append('~', position - rangeIndex);
            sb.Append('^');
            sb.Append('~', rangeLength - (position - rangeIndex) - 1);

            throw new InvalidOperationException(sb.ToString());
        }
    }
}
