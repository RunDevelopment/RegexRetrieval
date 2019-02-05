using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegexRetrieval.Queries;
using static Test.Queries.AssertQuery;

namespace Test.Queries
{
    [TestClass]
    public class TokenListTest
    {
        [TestMethod]
        public void Words()
        {
            var tokens = new List<QueryToken>();

            tokens.AddWord("foo");
            AssertQuery.AssertTokens(new[] { Word("foo") }, tokens);

            tokens.AddWord("bar");
            AssertQuery.AssertTokens(new[] { Word("foobar") }, tokens);

            tokens.AddWord("");
            AssertQuery.AssertTokens(new[] { Word("foobar") }, tokens);
        }

        [TestMethod]
        public void QMarksAndStars()
        {
            var tokens = new List<QueryToken>();

            tokens.AddStar();
            AssertQuery.AssertTokens(new[] { Star }, tokens);

            tokens.AddQMark();
            AssertQuery.AssertTokens(new[] { QMark, Star }, tokens);

            tokens.AddQMark();
            AssertQuery.AssertTokens(new[] { QMark, QMark, Star }, tokens);

            tokens.AddStar();
            AssertQuery.AssertTokens(new[] { QMark, QMark, Star }, tokens);
        }

        [TestMethod]
        public void CharSets()
        {
            var tokens = new List<QueryToken>();

            tokens.AddCharSet("abc");
            AssertQuery.AssertTokens(new[] { CharSet("abc") }, tokens);

            tokens.AddCharSet("d");
            AssertQuery.AssertTokens(new[] { CharSet("abc"), Word("d") }, tokens);
        }

        [TestMethod]
        public void Optionals()
        {
            var tokens = new List<QueryToken>();

            tokens.AddOptional("a");
            AssertQuery.AssertTokens(new[] { Optional("a") }, tokens);

            tokens.AddOptional("");
            AssertQuery.AssertTokens(new[] { Optional("a") }, tokens);
        }

        [TestMethod]
        public void StarsAndOptionals()
        {
            var tokens = new List<QueryToken>();

            tokens.AddStar().AddOptional("a").AddOptional("b");
            AssertQuery.AssertTokens(new[] { Star }, tokens);

            tokens.Clear();
            tokens.AddOptional("a").AddOptional("b").AddStar();
            AssertQuery.AssertTokens(new[] { Star }, tokens);
        }

        [TestMethod]
        public void QMArksAndStarsAndOptionals()
        {
            var tokens = new List<QueryToken>();

            tokens.AddStar().AddQMark().AddOptional("a").AddOptional("b");
            AssertQuery.AssertTokens(new[] { QMark, Star }, tokens);

            tokens.Clear();
            tokens.AddOptional("a").AddQMark().AddOptional("b").AddQMark().AddStar();
            AssertQuery.AssertTokens(new[] { QMark, QMark, Star }, tokens);
        }
    }
}
