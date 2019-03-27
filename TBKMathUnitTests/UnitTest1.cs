using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TBKMath;

namespace TBKMathUnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            int nTests = 4;
            int[] k = new int[] { 3, 3, 3, 3 };
            int[] i = new int[] { 3, 4, 5, 6 };
            int[] j = new int[] { 4, 5, 6, 7 };
            int[] x = new int[] { 1, -1, -1, 1 };

            for (int test = 0; test < nTests; test++)
            {
                int y = Utilities.DesignSign(i[test], j[test], k[test]);
                Assert.AreEqual(x[test], y);
            }
        }
    }
}
