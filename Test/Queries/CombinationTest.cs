using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegexRetrieval.Queries;

namespace Test.Queries
{
    [TestClass]
    public class CombinationTest
    {
        private static Query CreateQuery(Action<List<QueryToken>> adder)
        {
            var tokens = new List<QueryToken>();
            adder(tokens);
            return new Query(tokens);
        }

        private static void AssertCombinations(Query query, IEnumerable<string> combinations)
        {
            var actual = new HashSet<string>(query.GetCombinations());
            var expected = new HashSet<string>(combinations);
            var expectedCopy = new HashSet<string>(expected);

            expected.ExceptWith(actual);
            actual.ExceptWith(expectedCopy);

            if (expected.Count > 0 || actual.Count > 0)
            {
                Assert.Fail($@"Expected combinations of query and actual combinations are not identical!
Actual without expected: ""{string.Join("\", \"", actual)}""
Expected without actual: ""{string.Join("\", \"", expected)}""");
            }
        }

        [TestMethod]
        public void BasicTest()
        {
            AssertCombinations(
                CreateQuery(t => t.AddWord("foo").AddOptional("a").AddCharSet("xyz")),
                new string[]
                {
                    "foox",
                    "fooy",
                    "fooz",
                    "fooax",
                    "fooay",
                    "fooaz",
                }
            );
        }
    }
}
