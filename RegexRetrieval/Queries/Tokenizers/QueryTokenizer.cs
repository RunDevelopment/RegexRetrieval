﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using static RegexRetrieval.Queries.Tokenizers.QueryTokenizerUtil;

namespace RegexRetrieval.Queries.Tokenizers
{
    public class QueryTokenizer : IQueryTokenizer
    {
        private static readonly Regex QMark = ToStickyRegExp(@"\?");
        private static readonly Regex Star = ToStickyRegExp(@"\*");

        private static readonly Regex Square = ToStickyRegExp(@"\[((?:[^\\]|\\[\s\S])*?)\]");
        private static readonly Regex Round = ToStickyRegExp(@"\{((?:[^\\]|\\[\s\S])*?)\}");

        private readonly Tokenizer<State> tokenizer = CreateTokenizer();

        private QueryTokenizer() { }

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
            parser.AddCase(Square, (state, query, m) =>
            {
                var set = TokenUtil.UniqueCharSet(UnescapeWithParserError(query, m.Groups[1]));
                state.Tokens.AddCharSet(set);
            });
            parser.AddCase(Round, (state, query, m) =>
            {
                var opt = UnescapeWithParserError(query, m.Groups[1]);
                state.Tokens.AddOptional(opt);
            });

            parser.Default = (state, query, position) =>
            {
                char c = query[position];

                // invalid characters & parsing errors
                if (c == '(') throw TokenizerError("Cannot parse optional", query, position);
                if (c == '[') throw TokenizerError("Cannot parse char set", query, position);
                if (!IsValidCharacter(c) && c != '\\') throw TokenizerError("Invalid character", query, position);

                // get word
                int p = position;
                for (; p < query.Length; p++)
                {
                    c = query[p];
                    if (c == '\\') p++;
                    else if (!IsValidCharacter(c)) break;
                }

                if (p > query.Length) throw TokenizerError("Invalid escape", query, query.Length - 1);

                int length = position - p;
                var word = Unescape(query.Substring(position, length));

                state.Tokens.AddWord(word);
                return length;
            };

            return parser;
        }

        private static string Unescape(string value)
        {
            if (value.Length < 2 || !value.Contains('\\')) return value;

            var sb = new StringBuilder(value.Length);

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];

                if (c == '\\')
                    sb.Append(value[++i]);
                else
                    sb.Append(c);
            }

            return sb.ToString();
        }
        private static string UnescapeWithParserError(string query, Capture capture)
        {
            try
            {
                return Unescape(capture.Value);
            }
            catch (Exception e)
            {
                throw TokenizerError("Invalid escape", query, capture.Index, rangeLength: capture.Length);
            }
        }
        private static bool IsValidCharacter(char c)
            => @"?*+(){}[]|\".IndexOf(c) == -1;


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

        public static readonly QueryTokenizer Instance = new QueryTokenizer();
    }
}