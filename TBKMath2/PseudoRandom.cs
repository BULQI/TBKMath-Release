using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Troschuetz.Random;

namespace TBKMath
{
    class PseudoRandom
    {
    }

    static class DiscreteDistribution
    {
        private static Troschuetz.Random.Generators.MT19937Generator mt;

        static DiscreteDistribution()
        {
            mt = new Troschuetz.Random.Generators.MT19937Generator();
        }

        public static int Next(double[] _p)
        {
            double theta = mt.NextDouble();
            int returnValue = 0;
            double cum = _p[returnValue];
            while (theta > cum)
            {
                returnValue++;
                cum += _p[returnValue];
            }
            return returnValue;
        }
    }

    public class GeneralDiscreteDistribution<T>
    {
        private double[] probabilities;
        private T[] items;
        private double[] cumulatives;
        Dictionary<T, double> Probability;
        private static Troschuetz.Random.Generators.MT19937Generator generator;
        private static Troschuetz.Random.Distributions.Continuous.ContinuousUniformDistribution uniGen;
        static GeneralDiscreteDistribution()
        {
            generator = new Troschuetz.Random.Generators.MT19937Generator();
            uniGen = new Troschuetz.Random.Distributions.Continuous.ContinuousUniformDistribution(generator);
        }

        public GeneralDiscreteDistribution(T[] _items, double[] _probabilities)
        {
            probabilities = (double[])_probabilities.Clone();
            items = (T[])_items.Clone();

            if (items.Length != probabilities.Length)
            {
                throw new ArgumentException("Argument lengths are not the same.");
            }

            Array.Sort(probabilities, items);
            Array.Reverse(probabilities);
            Array.Reverse(items);

            cumulatives = new double[probabilities.Length];

            cumulatives[0] = probabilities[0];
            for (int i = 1; i < cumulatives.Length; i++)
            {
                cumulatives[i] = cumulatives[i - 1] + probabilities[i];
            }

            Probability = new Dictionary<T, double>();
            for (int i = 0; i < probabilities.Length; i++)
            {
                Probability.Add(items[i], probabilities[i]);
            }
        }

        public T Next()
        {
            double u = uniGen.NextDouble();
            for (int i = 0; i < cumulatives.Length; i++)
            {
                if (u <= cumulatives[i])
                {
                    return items[i];
                }
            }
            return items.Last();
        }
    }
}
