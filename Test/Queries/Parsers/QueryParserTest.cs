using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegexRetrieval.Queries;
using RegexRetrieval.Queries.Parsers;
using static Test.Queries.AssertQuery;

namespace Test.Queries.Parsers
{
    [TestClass]
    public class QueryParserTest
    {
        private readonly Dictionary<string, IList<QueryToken>> TestCases = new Dictionary<string, IList<QueryToken>>()
        {
            [""] = Array.Empty<QueryToken>(),

            ["abc"] = new[] { Word("abc") },
            ["?"] = new[] { QMark },
            ["*"] = new[] { Star },
            ["*?*?*"] = new[] { QMark, QMark, Star },

            ["[a]"] = new[] { Word("a") },
            ["[ab]"] = new[] { CharSet("ab") },
            ["[abc]"] = new[] { CharSet("abc") },
            ["[abba]"] = new[] { CharSet("ab") },
            ["[aaaa]"] = new[] { Word("a") },

            ["()"] = Array.Empty<QueryToken>(),
            ["(a)"] = new[] { Optional("a") },
            ["(abc)"] = new[] { Optional("abc") },
        };

        [TestMethod]
        public void QueriesTest()
        {
            AssertQueries(TestCases, QueryParser.Instance);
        }
    }
}
