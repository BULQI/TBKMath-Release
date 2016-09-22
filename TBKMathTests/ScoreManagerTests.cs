using Microsoft.VisualStudio.TestTools.UnitTesting;
using TBKMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBKMath.Tests
{
    [TestClass()]
    public class ScoreManagerTests
    {
        [TestMethod()]
        public void ScoreManagerTest()
        {
            ScoreManager<string> sm = new ScoreManager<string>();
            Assert.IsInstanceOfType(sm, typeof(ScoreManager<string>));
        }

        [TestMethod()]
        public void AddTest()
        {
            ScoreManager<string> sm = new ScoreManager<string>();
            sm.Threshold = 1;
            bool added = sm.Add("one", 1);
            Assert.IsTrue(added);
            Assert.AreEqual(sm.Count, 1);
            Assert.AreEqual(sm.TopScore, 1);
            added = sm.Add("one-half", 0.5);
            Assert.IsTrue(added);
            Assert.AreEqual(sm.Count, 2);
            added = sm.Add("minus one", -1);
            Assert.IsFalse(added);
            Assert.AreEqual(sm.Count, 2);
            added = sm.Add("two", 2);
            Assert.IsTrue(added);
            Assert.AreEqual(sm.Count, 3);
            Assert.AreEqual(sm.TopScore, 2);
            Assert.AreEqual(sm.TopItem, "two");
            added = sm.Add("one", 3);
            Assert.IsFalse(added);
        }

        [TestMethod()]
        public void AddTest1()
        {
            ScoreManager<string> sm = new ScoreManager<string>();
            sm.Threshold = 1;
            bool added = sm.Add(new ScoredObject<string>() { Item = "one", Score = 1 });
            Assert.IsTrue(added);
            Assert.AreEqual(sm.Count, 1);
            Assert.AreEqual(sm.TopScore, 1);
            added = sm.Add(new ScoredObject<string>() { Item = "one-half", Score = 0.5 });
            Assert.IsTrue(added);
            Assert.AreEqual(sm.Count, 2);
            added = sm.Add(new ScoredObject<string>() { Item = "minus-one", Score = -1 });
            Assert.IsFalse(added);
            Assert.AreEqual(sm.Count, 2);
            added = sm.Add(new ScoredObject<string>() { Item = "two", Score = 2 });
            Assert.IsTrue(added);
            Assert.AreEqual(sm.Count, 3);
            Assert.AreEqual(sm.TopScore, 2);
            Assert.AreEqual(sm.TopItem, "two");
            added = sm.Add(new ScoredObject<string>() { Item = "one", Score = 3});
            Assert.IsFalse(added);
        }

        [TestMethod()]
        public void ExponentiateAndNormalizeTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetBayesFactorTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void PurgeTest()
        {
            ScoreManager<string> sm = new ScoreManager<string>();
            sm.Threshold = 0.99;
            bool added = sm.Add("zero", 0);
            added = sm.Add("one-half", 0.5);
            added = sm.Add("one", 1);
            Assert.AreEqual(sm.Count, 3);
            sm.Purge();
            Assert.AreEqual(sm.Count, 2);
        }

        [TestMethod()]
        public void RevalidateTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DeactivateAtTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DeactivateTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ActivateTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ItemsWithScoresGreaterThanTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ScoredItemsWithScoresGreaterThanTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void RecomputeTopScoreTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetEnumeratorTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ScoreManagerTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void AddTest2()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void AddTest3()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ExponentiateAndNormalizeTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetBayesFactorTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void PurgeTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void RevalidateTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DeactivateTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ActivateTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ItemsWithScoresGreaterThanTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ScoredItemsWithScoresGreaterThanTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void RecomputeTopScoreTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ForEachTest()
        {
            ScoreManager<string> sm = new ScoreManager<string>();
            sm.Threshold = 1.1;
            sm.Add("zero", 0);
            sm.Add("1", 1);
            sm.Add("2", 2);

            List<string> expecteditems = new List<string>() { "zero", "1", "2" };
            List<string> items = new List<string>();
            foreach(string item in sm)
            {
                items.Add(item);
            }

            for (int i= 0; i< 3; i++)
            {
                Assert.AreEqual(items[i], expecteditems[i]);
            }

            sm.Revalidate();
            items.Clear();
            foreach (string item in sm)
            {
                items.Add(item);
            }

            expecteditems.Remove("zero");
            for (int i = 0; i < 2; i++)
            {
                Assert.AreEqual(items[i], expecteditems[i]);
            }

        }
    }
}