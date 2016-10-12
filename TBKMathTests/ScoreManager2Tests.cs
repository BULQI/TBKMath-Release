using Microsoft.VisualStudio.TestTools.UnitTesting;
using TBKMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBKMath.Tests
{
    [TestClass()]
    public class ScoreManager2Tests
    {
        [TestMethod()]
        public void ScoreManager2Test()
        {
            ScoreManager2<string> sm = new ScoreManager2<string>();
            Assert.IsInstanceOfType(sm, typeof(ScoreManager2<string>));
        }

        [TestMethod()]
        public void AddTest()
        {
            ScoreManager2<string> sm = new ScoreManager2<string>(useNames: true);
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
            ScoreManager2<string> sm = new ScoreManager2<string>(useNames: true);
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
            added = sm.Add(new ScoredObject<string>() { Item = "one", Score = 3 });
            Assert.IsFalse(added);
        }

        [TestMethod()]
        public void ExponentiateAndNormalizeTest()
        {
            ScoreManager2<string> sm = new ScoreManager2<string>();
            sm.Add("first", -3);
            sm.Add("second", -2);
            sm.Add("third", -1);
            sm.ExponentiateAndNormalize();
            double sum = 0;
            foreach (ScoredObject<string> so in sm)
            {
                sum += so.Score;
            }
            Assert.AreEqual(sum, 1, 1E-6);
        }

        [TestMethod()]
        public void SortTest()
        {
            ScoreManager2<string> sm = new ScoreManager2<string>();
            sm.Threshold = 10;
            List<int> ordered = new List<int>() { 0, -1, -3 };
            sm.Add("first", -3);
            sm.Add("second", 0);
            sm.Add("third", -1);
            sm.Sort();
            int i = 0;
            foreach (ScoredObject<string> so in sm)
            {
                Assert.AreEqual(ordered[i], so.Score);
                i++;
            }
        }

        [TestMethod()]
        public void GetBayesFactorTest()
        {
            ScoreManager2<string> sm = new ScoreManager2<string>();
            sm.Add("first", -3);
            sm.Add("second", 0);
            sm.Add("third", -1);
            double bayesFactor = sm.GetBayesFactor();
            Assert.AreEqual(0.34901221676818633, bayesFactor, 1E-6);
        }

        [TestMethod()]
        public void PurgeTest()
        {
            ScoreManager2<string> sm = new ScoreManager2<string>();
            sm.Threshold = 0.99;
            bool added = sm.Add("zero", 0);
            added = sm.Add("one-half", 0.5);
            added = sm.Add("one", 1);
            Assert.AreEqual(sm.Count, 3);
            sm.Purge();
            Assert.AreEqual(sm.Count, 2);
        }

        [TestMethod()]
        public void RemoveTest()
        {
            ScoreManager2<string> sm = new ScoreManager2<string>(useNames: true);
            sm.Threshold = 10;
            sm.Add("first", -3);
            sm.Add("second", 0);
            sm.Add("third", -1);
            Assert.AreEqual(sm.Count, 3);
            sm.Remove("first");
            Assert.AreEqual(sm.Count, 2);
            bool contains = sm.ContainsItemNamed("first");
            Assert.IsFalse(contains);
            sm.Remove("third");
            Assert.AreEqual(sm.Count, 1);
            contains = sm.ContainsItemNamed("third");
            Assert.IsFalse(contains);
            sm.Remove("second");
            Assert.AreEqual(sm.Count, 0);
            contains = sm.ContainsItemNamed("second");
            Assert.IsFalse(contains);
        }

        [TestMethod()]
        public void RevalidateTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void RecomputeTopScoreTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ContainsItemNamedTest()
        {
            ScoreManager2<string> sm = new ScoreManager2<string>(useNames: true);
            sm.Threshold = 10;
            sm.Add("first", -3);
            sm.Add("second", 0);
            sm.Add("third", -1);
            Assert.AreEqual(sm.Count, 3);
            bool contains = sm.ContainsItemNamed("second");
            Assert.IsTrue(contains);
            contains = sm.ContainsItemNamed("fourth");
            Assert.IsFalse(contains);
        }

        [TestMethod()]
        public void ItemByNameTest()
        {
            ScoreManager2<int> sm = new ScoreManager2<int>(useNames: true);
            sm.Threshold = 10;
            sm.Add(0, -3);
            sm.Add(2, 0);
            sm.Add(1, -1);
            Assert.AreEqual(sm.Count, 3);
            int item = sm.ItemByName("0");
            Assert.AreEqual(item, 0);
            item = sm.ItemByName("2");
            Assert.AreEqual(item, 2);
        }
    }
}