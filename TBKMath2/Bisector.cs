using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBKMath
{
    public class Bisector
    {
        public delegate double FunctionDelegate(double x);
        public FunctionDelegate func;
        public int iterations;

        public double tolerance = 1.0E-6;

        public Bisector(FunctionDelegate funcDel)
        {
            this.func = funcDel;
        }

        private double[] bracket(double left, double right)
        {
            if (left >= right)
            {
                throw new ArgumentException("Left is greater than or equal to right.");
            }

            double a = left;
            double b = right;
            double fa = func(a);
            double fb = func(b);

            if (fa * fb <= 0)
            {
                return new double[] { a, b };
            }
            
            if (fa > fb)
            {
                if (fa > 0)
                {
                    while (fa * fb > 0)
                    {
                        b = 2 * b - a;
                        fb = func(b);
                    }
                }
                else
                {
                    while (fa * fb > 0)
                    {
                        a = 2 * a - b;
                        fa = func(a);
                    }
                }
            }
            else
            {
                if (fa < 0)
                {
                    while (fa * fb > 0)
                    {
                        b = 2 * b - a;
                        fb = func(b);
                    }
                }
                else
                {
                    while (fa * fb > 0)
                    {
                        a = 2 * a - b;
                        fa = func(a);
                    }
                }
            }
            return new double[] { a, b };
        }

        public double getRoot(double left, double right)
        {
            if (func == null)
            {
                throw new ArgumentNullException("The function is null.");
            }
            if (left >= right)
            {
                throw new ArgumentException("left is not less than right, as required.");
            }

            double fl = func(left);
            double fr = func(right);
            double fm;

            if (fl * fr > 0)
            {
                double[] bounds = bracket(left, right);
                left = bounds[0];
                right = bounds[1];
            }

            iterations = 0;
            while (Math.Abs(right - left) > 2 * tolerance)
            {
                double midpoint = (right + left) / 2;
                fm = func(midpoint);
                if ((fl * fm) > 0)
                {
                    left = midpoint;
                    fl = fm;
                }
                else
                {
                    right = midpoint;
                    fr = fm;
                }
                iterations++;
            }
            return (right + left) / 2;
        }


    }

}
