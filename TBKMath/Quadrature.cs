using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBKMath
{
    public static class Quadrature
    {
        public static double Simpson(double a, double b, Func<double, double> func)
        {
            return (b - a) / 6 * (func(a) + 4 * func((a + b) / 2) + func(b));
        }
    }
}
