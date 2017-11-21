using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TBKMath;

namespace TestMathLibrary
{
    class TestFunctions
    {
        public static double parabola(List<double> parameters)
        {
            return Math.Pow(parameters[1] - 1, 2) + Math.Pow(parameters[0] + 1, 2);
        }

        public static double xMinusLogXMinusTwo(double x)
        {
            return x - Math.Log(x) - 2;
        }

        public static Tree<string> RerootTree(Tree<string> tree, string newRoot)
        {
            Tree<string> nuRoot = Tree<string>.GetDescendant(tree, newRoot);
            nuRoot.RootHere();
            return nuRoot;
        }

    }

    public class TestStatistics
    {
        private double[] x;
        private double[] y;
        private int n;

        public double LogLikelihoodLR(double[] pars)
        {
            double alpha = pars[0];
            double beta = pars[1];
            double tau = Math.Exp(pars[2]);

            double SS = 0;
            for (int i = 0; i < n; i++)
            {
                SS += Math.Pow(y[i] - alpha - beta * x[i], 2);
            }
            return -0.5 * n * Math.Log(tau) - 0.5 * SS / tau;
        }

        public double LogPriorLR(double[] pars)
        {
            return 0;
        }

        public void LoadData(double[] x, double[] y)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException("The x and y vectors must be the same length.");
            }
            n = x.Length;
            this.x = x;
            this.y = y;
        }
    }
}
