using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static RegexRetrieval.Queries. QueryTokenizerUtil;

namespace RegexRetrieval.Queries
{
    public class NetspeakQueryTokenizer : IQueryTokenizer
    {
        private static readonly Regex QMark = ToStickyRegExp(@"\?");
        private static readonly Regex Star = ToStickyRegExp(@"\*");
        private static readonly Regex Plus = ToStickyRegExp(@"\+");

        private static readonly Regex Curly = ToStickyRegExp(@"\{([\s\S]*?)\}");
        private static readonly Regex Square = ToStickyRegExp(@"\[([\s\S]*?)\]");

        private readonly Tokenizer<State> tokenizer = CreateTokenizer();

        private NetspeakQueryTokenizer() { }

        private static Tokenizer<State> CreateTokenizer()
        {
            var parser = new Tokenizer<State>();

            parser.AddCase(QMark, (state, query, m) =>
            {
                state.Tokens.AddQMark();
            });
            parser.AddCase(Star, (state, query, m) =>
            {
                state.Tokens.AddStar();
            });
            parser.AddCase(Plus, (state, query, m) =>
            {
                state.Tokens.AddQMark().AddStar();
            });

            parser.AddCase(Curly, (state, query, m) =>
            {
                CheckSetCharacters(m.Groups[1], query);

                // {abc} == regex: /[abc][abc][abc]/
                var set = TokenUtil.UniqueCharSet(m.Groups[1].Value);
                var length = m.Groups[1].Length;

                if (length == 0) return; // nothing to do here

                if (set.Length == 1)
                {
                    // optimize of the case of only one character in the set
                    state.Tokens.AddWord(new string(set[0], length));
                }
                else
                {
                    for (var i = 0; i < length; i++)
                        state.Tokens.AddCharSet(set);
                }
            });
            parser.AddCase(Square, (state, query, m) =>
            {
                var group = m.Groups[1];

                if (group.Length == 0)
                    throw TokenizerError("There has to be at least one character", query, m.Index, rangeLength: 2);
                CheckSetCharacters(group, query);

                if (group.Length == 1)
                {
                    // [a] == regex: /(?:a)?/
                    var set = group.Value;

                    state.Tokens.AddOptional(set);
                }
                else
                {
                    // [abc] == regex: /[abc]/
                    var set = TokenUtil.UniqueCharSet(group.Value);

                    if (set.Length == 1)
                        state.Tokens.AddWord(set);
                    else
                        state.Tokens.AddCharSet(set);
                }
            });

            parser.Default = (state, query, position) =>
            {
                char c = query[position];

                if (!IsValidCharacter(c))
                    throw TokenizerError($"Invalid character '{c}'", query, position);

                int index = position;
                int len = 1;
                while (++position < query.Length && IsValidCharacter(query[position]))
                    len++;

                state.Tokens.AddWord(query.Substring(index, len));
                return len;
            };

            return parser;
        }

        private static void CheckSetCharacters(Group g, string query)
        {
            var set = g.Value;
            for (var i = 0; i < set.Length; i++)
            {
                char c = set[i];
                if (!IsValidCharacter(c))
                    throw TokenizerError($"Invalid character '{c}'", query, g.Index + i, g.Index, g.Length);
            }
        }
        private static bool IsValidCharacter(char c)
            => @"?*+(){}[]|".IndexOf(c) == -1;


        public List<QueryToken> Tokenize(string query)
        {
            var state = new State();
            tokenizer.Tokenize(query, state);

            return state.Tokens;
        }

        private class State
        {
            public List<QueryToken> Tokens = new List<QueryToken>(16);
        }

        public static readonly NetspeakQueryTokenizer Instance = new NetspeakQueryTokenizer();
    }
}
