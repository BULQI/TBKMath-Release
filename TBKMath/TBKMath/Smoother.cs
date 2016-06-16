using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBKMath
{
    public class Smoother
    {
        public static double[] Smooth(double[] x, double[] y, double[] newX, double h)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException("x and y must be the same length.");
            }

            double denom = 2*h*h;
            double[] normalization = new double[newX.Length];
            for (int i = 0; i < newX.Length; i++)
            {
                normalization[i] = 0;
                for (int j = 0; j < x.Length; j++)
                {
                    normalization[i] += Math.Exp(-Math.Pow(newX[i] - x[j], 2)/denom);
                }
                normalization[i] = 1.0 / normalization[i];
            }

            double[] smoothed = new double[newX.Length];
            for (int i = 0; i < smoothed.Length; i++)
            {
                for (int j = 0; j < x.Length; j++)
                {
                    smoothed[i] += normalization[i] * Math.Exp(-Math.Pow(newX[i] - x[j], 2) / denom) * y[j];
                }
            }

            return smoothed;
        }
    }
}
