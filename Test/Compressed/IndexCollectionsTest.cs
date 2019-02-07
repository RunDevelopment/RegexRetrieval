using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegexRetrieval.Compressed;

namespace Test.Compressed
{
    [TestClass]
    public class IndexCollectionsTest
    {
        private static readonly IList<int[]> TestCases = new int[][]
        {
            new int[] { },
            new int[] { 0 },
            new int[] { int.MaxValue },
            new int[] { 0, int.MaxValue },

            new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 },
            new int[] { 0, 2, 3, 4, 5, 6, 7, 8 },
            new int[] { 1, 2, 3, 4, 5, 6, 7, 8 },
            new int[] { 1, 2, 3, 5, 6, 7, 8 },
            new int[] { 1, 2, 3, 5, 6, 7, 8, 10 },
        };

        private static void Test(Func<int[], int[]> compressDecompress)
        {
            foreach (var expected in TestCases)
            {
                void Fail(string msg)
                {
                    msg += $"\nFor {{ {string.Join(", ", expected)} }}";
                    Assert.Fail(msg);
                }


                var actual = compressDecompress(expected);

                if (expected.Length != actual.Length)
                    Fail($"Different length.\nExpected: {expected.Length}, Actual: {actual.Length}");

                for (int i = 0; i < expected.Length; i++)
                {
                    var e = expected[i];
                    var a = actual[i];

                    if (e != a)
                        Fail($"Different value at {i}.\nExpected: {expected.Length}, Actual: {actual.Length}");
                }
            }
        }

        [TestMethod]
        public void CompressedIndexCollectionTest()
        {
            Test(c => new CompressedIndexCollection(c).ToArray());
        }

        [TestMethod]
        public void RunIndexCollectionTest()
        {
            Test(c => new RunIndexCollection(c).ToArray());
        }
    }
}
