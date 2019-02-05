using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegexRetrieval.Queries;
using static RegexRetrieval.Queries.QueryToken;

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
            AssertQuery.AssertTokens(new[] { CreateWord("foo") }, tokens);

            tokens.AddWord("bar");
            AssertQuery.AssertTokens(new[] { CreateWord("foobar") }, tokens);

            tokens.AddWord("");
            AssertQuery.AssertTokens(new[] { CreateWord("foobar") }, tokens);
        }

        [TestMethod]
        public void QMarksAndStars()
        {
            var tokens = new List<QueryToken>();

            tokens.AddStar();
            AssertQuery.AssertTokens(new[] { CreateStar() }, tokens);

            tokens.AddQMark();
            AssertQuery.AssertTokens(new[] { CreateQMark(), CreateStar() }, tokens);

            tokens.AddQMark();
            AssertQuery.AssertTokens(new[] { CreateQMark(), CreateQMark(), CreateStar() }, tokens);

            tokens.AddStar();
            AssertQuery.AssertTokens(new[] { CreateQMark(), CreateQMark(), CreateStar() }, tokens);
        }

        [TestMethod]
        public void CharSets()
        {
            var tokens = new List<QueryToken>();

            tokens.AddCharSet("abc");
            AssertQuery.AssertTokens(new[] { CreateCharSet("abc") }, tokens);

            tokens.AddCharSet("d");
            AssertQuery.AssertTokens(new[] { CreateCharSet("abc"), CreateWord("d") }, tokens);
        }

        [TestMethod]
        public void Optionals()
        {
            var tokens = new List<QueryToken>();

            tokens.AddOptional("a");
            AssertQuery.AssertTokens(new[] { CreateOptional("a") }, tokens);

            tokens.AddOptional("");
            AssertQuery.AssertTokens(new[] { CreateOptional("a") }, tokens);
        }

        [TestMethod]
        public void StarsAndOptionals()
        {
            var tokens = new List<QueryToken>();

            tokens.AddStar().AddOptional("a").AddOptional("b");
            AssertQuery.AssertTokens(new[] { CreateStar() }, tokens);

            tokens.Clear();
            tokens.AddOptional("a").AddOptional("b").AddStar();
            AssertQuery.AssertTokens(new[] { CreateStar() }, tokens);
        }

        [TestMethod]
        public void QMArksAndStarsAndOptionals()
        {
            var tokens = new List<QueryToken>();

            tokens.AddStar().AddQMark().AddOptional("a").AddOptional("b");
            AssertQuery.AssertTokens(new[] { CreateQMark(), CreateStar() }, tokens);

            tokens.Clear();
            tokens.AddOptional("a").AddQMark().AddOptional("b").AddQMark().AddStar();
            AssertQuery.AssertTokens(new[] { CreateQMark(), CreateQMark(), CreateStar() }, tokens);
        }
    }
}
