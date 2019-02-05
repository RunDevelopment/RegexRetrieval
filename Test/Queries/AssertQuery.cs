using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegexRetrieval.Queries;
using RegexRetrieval.Queries.Parsers;

namespace Test.Queries
{
    public static class AssertQuery
    {
        public static void AssertTokens(IList<QueryToken> expected, IList<QueryToken> actual)
        {
            AssertTokens(expected, actual, null);
        }
        public static void AssertTokens(IList<QueryToken> expected, IList<QueryToken> actual, string message)
        {
            void AppendMsg(string msg)
            {
                if (message == null) message = msg;
                else message = message + "\n" + msg;
            }

            if (expected == null || actual == null)
            {
                AppendMsg($"Both expected and actual token list have to be non null.");
                Assert.Fail(message);
            }

            if (expected.Count != actual.Count)
            {
                AppendMsg($"Different number of tokens.");
                Assert.AreEqual(expected.Count, actual.Count, message);
            }

            for (var i = 0; i < expected.Count; i++)
            {
                var e = expected[i];
                var a = actual[i];

                if (e.TokenType != a.TokenType)
                {
                    AppendMsg($"Different token types at {i}.");
                    Assert.AreEqual(e.TokenType, a.TokenType, message);
                }
                if (e.Value != a.Value)
                {
                    AppendMsg($"Different token values at {i}.");
                    Assert.AreEqual(e.Value, a.Value, message);
                }
            }
        }


        public static QueryToken Word(string value) => QueryToken.CreateWord(value);
        public static QueryToken QMark { get; } = QueryToken.CreateQMark();
        public static QueryToken Star { get; } = QueryToken.CreateStar();
        public static QueryToken CharSet(string value) => QueryToken.CreateCharSet(value);
        public static QueryToken Optional(string value) => QueryToken.CreateOptional(value);


        public static void AssertQueries(IEnumerable<KeyValuePair<string, IList<QueryToken>>> cases, IQueryParser parser)
        {
            foreach (var item in cases)
            {
                try
                {
                    AssertTokens(item.Value, parser.Parse(item.Key));
                }
                catch (AssertFailedException e)
                {
                    throw new AssertFailedException($"Failed for query: \"{item.Key}\"\n{e.Message}", e);
                }
            }
        }
        public static void AssertQueries(IEnumerable<KeyValuePair<string, Type>> cases, IQueryParser parser)
        {
            void AssertThrowsException(Type t, Action action, string msg)
            {
                Func<Action, string, Exception> assert = Assert.ThrowsException<Exception>;
                var generic = assert.Method.GetGenericMethodDefinition().MakeGenericMethod(new[] { t });
                generic.Invoke(null, new object[] { action, msg });
            }

            foreach (var item in cases)
            {
                AssertThrowsException(item.Value, () => parser.Parse(item.Key),
                    $"\"{item.Key}\" was expected to throw a {item.Value.Name}.");
            }
        }
    }
}
