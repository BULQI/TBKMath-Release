using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBKMath
{
    public class Quadrature
    {
        public Quadrature()
        {
        }

        public double Simpson(double a, double b)
        {
            return (b - a) / 6 * (func(a) + 4 * func((a + b) / 2) + func(b));
        }

        public delegate double IntegrandDelegate(double x);
        public IntegrandDelegate func;
    }
}
