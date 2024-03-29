﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TBKMath;

namespace TestMathLibrary
{
    class TestAmoeba
    {
        public TestAmoeba()
        {
            double tol = 0.001;
            am = new Amoeba();
            am.ToleranceRequested = tol;
            am.Function = TestFunctions.parabola;
        }

        public bool runTest()
        {
            am.MaxNumIterations = 1000;

            List<double> point = new List<double> { 0, 0 };
            am.Point = point;

            List<double> scale = new List<double> { 0.5, 0.5 };
            am.Scale = scale;

            am.Start();
            return true;
        }

        public List<double> argMin;
        public TBKMath.Amoeba am;        
    }

    class TestCodeCogs
    {
        public TestCodeCogs() { }

        public void runTest()
        {
            double y = TBKMath.SpecialFunctions.Psi(0.5);
        }

    }

    class TestBisector
    {
        public TestBisector()
        {
            bi = new Bisector(new Bisector.FunctionDelegate(TestFunctions.xMinusLogXMinusTwo));
        }

        public void runTest()
        {
            double left = 1;
            double right = 10;
            argMin = bi.getRoot(left, right);
        }
        public TBKMath.Bisector bi;
        public double argMin;
    }

    class TestMCMC
    {
        public TBKMath.MCMC mcmc;
        public TestMCMC()
        {
            mcmc = new MCMC();
        }

        public void RunTest()
        {
            
        }
    }

    class TestSemipartition
    {
        Semipartition<int> semipartition;
        public TestSemipartition()
        {
        }

        public void Test()
        {
            reset();
            semipartition.PurgeSubblocks();
            Tuple<Semipartition<int>, Partition<int>> decomposition = semipartition.Decompose();
            Partition<int> partition = decomposition.Item1.MergeOverlappingBlocks();
            reset();
            semipartition.MergeAllBlocks();
            reset();
            partition = semipartition.MergeOverlappingBlocks();
            Partition<int> newPartition = new Partition<int>();
            newPartition.AddBlock(new HashSet<int>() { 21, 22, 23 });
            Partition<int> joined = Partition<int>.Join(partition, newPartition);
        }

        private void reset()
        {
            semipartition = new Semipartition<int>();
            semipartition.AddBlock(new HashSet<int>() { 1, 2});
            semipartition.AddBlock(new HashSet<int>() { 3, 4 });
            semipartition.AddBlock(new HashSet<int>() { 5, 6 });
            semipartition.AddBlock(new HashSet<int>() { 6, 7 });
            semipartition.AddBlock(new HashSet<int>() { 8, 9 });
            semipartition.AddBlock(new HashSet<int>() { 10, 11 });
            semipartition.AddBlock(new HashSet<int>() { 2, 3 });
            semipartition.AddBlock(new HashSet<int>() { 2, 3 });
            semipartition.AddBlock(new HashSet<int>() { 1 });
        }
        
    }

    static class TestScoreManager
    {
        public static double[] Test(int totalSize)
        {
            ScoreManager2<int> manager = new ScoreManager2<int>();
            Random random = new Random();
            manager.Threshold = 10;
            for (int i = 0; i < totalSize; i++)
            {
                manager.Add(i, random.NextDouble());
            }
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            manager.Threshold = 0.5;
            manager.Purge();
            timer.Stop();
            double purgeTime = timer.ElapsedMilliseconds;

            manager = new ScoreManager2<int>();
            manager.Threshold = 10;
            for (int i = 0; i < totalSize; i++)
            {
                manager.Add(i, random.NextDouble());
            }
            timer.Reset();
            timer.Start();
            manager.Threshold = 0.5;
            manager.Revalidate();
            timer.Stop();
            double revalTime = timer.ElapsedMilliseconds;

            return new double[] { purgeTime, revalTime };
        }


    }
}
