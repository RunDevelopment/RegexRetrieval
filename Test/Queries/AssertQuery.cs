using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegexRetrieval.Queries;

namespace Test.Queries
{
  public static  class AssertQuery
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

            if (expected == null ||actual == null)
            {
                AppendMsg($"Both expected and actual token list have to be non null.");
                Assert.  Fail(message);
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
    }
}
