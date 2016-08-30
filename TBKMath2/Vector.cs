using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBKMath
{
    public class Vector : ICloneable
    {
        public Vector()
        {

        }

        public Vector(int _dim)
        {
            dim = _dim;
            components = new double[_dim];
        }

        public Vector(List<double> _V)
        {
            dim = _V.Count;
            components = new double[dim];
            for (int i = 0; i < dim; i++)
            {
                components[i] = _V[i];
            }
        }

        public Vector(double[] _V)
        {
            dim = _V.Length;
            components = new double[dim];
            components = (double[])_V.Clone();
            dim = _V.Length;
        }

        public Vector(Vector _V)
        {
            dim = _V.dim;
            components = new double[dim];
            components = (double[])_V.components.Clone();
        }

        private double[] components;
        
        private int dim;
        public int Dim
        {
            get { return dim; }
        }

        public double this[int index]
        {
            get { return components[index]; }
            set { components[index] = value; }
        }

        public object Clone()
        {
            Vector vec = new Vector();
            vec.dim = this.dim;
            vec.components = new double[this.dim];
            for (int i = 0;i < this.dim; i++)
            {
                vec.components[i] = this.components[i];
            }
            return vec;
        }

        public static Vector operator *(double a, Vector x)
        {
            Vector value = new Vector(x.dim);
            for (int i = 0; i < x.dim; i++)
            {
                value[i] = x.components[i] * a;
            }
            return value;
        }

        public static Vector operator +(Vector x, Vector y)
        {
            Vector value = new Vector(x.dim);
            for (int i = 0; i < x.dim; i++)
            {
                value.components[i] = x[i] + y[i];
            }
            return value;
        }

        public static Vector operator -(Vector x, Vector y)
        {
            Vector value = new Vector(x.dim);
            for (int i = 0; i < x.dim; i++)
            {
                value.components[i] = x[i] - y[i];
            }
            return value;
        }

        public double Sum()
        {
            return this.components.Sum();
        }

        public static double Dot(Vector x, Vector y)
        {
            double returnValue = 0;
            for (int i = 0; i < x.dim; i++)
            {
                returnValue += x.components[i] * y.components[i];
            }

            return returnValue;
        }

        public double Dot(Vector x)
        {
            return Dot(x, this);
        }

        public static double EuclideanDistanceSquared(Vector x, Vector y)
        {
            Vector z = new Vector(x.dim);
            for (int i = 0; i < x.dim; i++)
            {
                z.components[i] = (x.components[i] - y.components[i]);
            }
            return SquaredModulus(z);
        }

        public double EuclideanDistanceSquared(Vector x)
        {
            return EuclideanDistanceSquared(x, this);
        }

        public static double SquaredModulus(Vector x)
        {
            return Dot(x, x);
        }

        public double SquaredModulus()
        {
            return SquaredModulus(this);
        }

        public double Mean()
        {
            return this.components.Average();
        }

        public double Variance()
        {
            double mean = this.Mean();
            return (SquaredModulus() - dim * mean * mean) / (dim - 1);
        }

        public double Max()
        {
            return this.components.Max();
        }

        public double Min()
        {
            return this.components.Min();
        }

        public override string ToString()
        {
            // returns a string containing the components of the vector
            // separated by tabs
            string[] _components = new string[components.Length];
            for (int i = 0; i < components.Length; i++)
            {
                _components[i] = components[i].ToString();
            }
            string value = _components[0];
            for (int i = 1; i < components.Length; i++)
            {
                value += "\t" + components[i];
            }
            return value;
        }
    }
}
