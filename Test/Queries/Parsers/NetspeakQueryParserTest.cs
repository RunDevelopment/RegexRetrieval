using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegexRetrieval.Queries;
using RegexRetrieval.Queries.Parsers;
using static Test.Queries.AssertQuery;

namespace Test.Queries.Parsers
{
    [TestClass]
    public class NetspeakQueryParserTest
    {
        private readonly Dictionary<string, IList<QueryToken>> TestCases = new Dictionary<string, IList<QueryToken>>()
        {
            [""] = Array.Empty<QueryToken>(),

            ["abc"] = new[] { Word("abc") },
            ["?"] = new[] { QMark },
            ["*"] = new[] { Star },
            ["+"] = new[] { QMark, Star },
            ["+++?*"] = new[] { QMark, QMark, QMark, QMark, Star },

            ["[a]"] = new[] { Optional("a") },
            ["[ab]"] = new[] { CharSet("ab") },
            ["[abc]"] = new[] { CharSet("abc") },
            ["[abba]"] = new[] { CharSet("ab") },
            ["[aaaa]"] = new[] { Word("a") },

            ["{a}"] = new[] { Word("a") },
            ["{abc}"] = new[] { CharSet("abc"), CharSet("abc"), CharSet("abc") },
            ["{abba}"] = new[] { CharSet("ab"), CharSet("ab"), CharSet("ab"), CharSet("ab") },
        };

        [TestMethod]
        public void QueriesTest()
        {
            AssertQueries(TestCases, NetspeakQueryParser.Instance);
        }
    }
}
