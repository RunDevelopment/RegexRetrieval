using System.Collections.Generic;

namespace RegexRetrieval.Cli
{
    internal static class Util
    {

        public static string[] GetWords(string text)
        {
            var newLines = 0;
            int lastNewLine = -2;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    if (i - lastNewLine > 1 && i != text.Length - 1) newLines++;
                    lastNewLine = i;
                }
            }

            var words = new string[newLines + 1];
            var wordsIndex = 0;
            var last = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    if (i > last)
                        words[wordsIndex++] = text.Substring(last, i - last);
                    last = i + 1;
                }
            }
            if (text.Length > last)
                words[wordsIndex] = text.Substring(last, text.Length - last);

            return words;
        }

        public static IEnumerable<object> GetInfos(this IInfoProvider provider)
        {
            foreach (var entry in provider.InfoMetadata)
            {
                if (provider.TryGetInfo(entry.Name, out var value))
                    yield return value;
                else
                    yield return null;
            }
        }
    }
}
