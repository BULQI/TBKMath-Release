// GNU General Public License Agreement
// Copyright (C) 2004-2007 CodeCogs, Zyba Ltd, Broadwood, Holford, TA5 1DU, England.
// adapted to c# by Thomas B Kepler kepler@duke.edu
// This program is free software; you can redistribute it and/or modify it under
// the terms of the GNU General Public License as published by CodeCogs. 
// You must retain a copy of this licence in all copies. 
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY
// WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
// PARTICULAR PURPOSE. See the GNU General Public License for more details.
// ---------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBKMath
{
    public class SpecialFunctions
    {
        public SpecialFunctions()
        {

        }

        public static double LogGamma(double x)
        {
            // Stirling's formula for the gamma function
            List<double> A = new List<double> 
            {
                8.11614167470508450300E-4,
                -5.95061904284301438324E-4,
                7.93650340457716943945E-4,
                -2.77777777730099687205E-3,
                8.33333333333331927722E-2
            };
            
            List<double> B = new List<double>
            {
            -1.37825152569120859100E3,
            -3.88016315134637840924E4,
            -3.31612992738871184744E5,
            -1.16237097492762307383E6,
            -1.72173700820839662146E6,
            -8.53555664245765465627E5
            };
          
            List<double> C = new List<double>
            {
            /* 1.00000000000000000000E0, */
            -3.51815701436523470549E2,
            -1.70642106651881159223E4,
            -2.20528590553854454839E5,
            -1.13933444367982507207E6,
            -2.53252307177582951285E6,
            -2.01889141433532773231E6
            };

            const double LOGPI = 1.14472988584940017414;
            const double LS2PI = 0.91893853320467274178;    /* log( sqrt( 2*pi ) ) */

            int lsign = 1;
            int sign = 1;
            double q,w,p,u,z;
            int i;

            if( x < -34.0 )
            {
                q = -x;
                w = LogGamma(q); /* note this modifies sign! */

                p = Math.Floor(q);
                if( p == q ) //  argument is negative integer, return NaN
                {            //  (each singularity goes to +/- infinity depending 
                             //  on direction of limit
                    return double.NaN;
                }
                
                i = (int)p;

                // need to translate this code:
                // is it computing the parity of i?
                lsign = 2*(i&1) -1;
                ///////////////////////////////
                sign = lsign;

                z = q - p;
                if( z > 0.5 )
                {
                    p ++;
                    z = p - q;
                }

                z = q * Math.Sin( Math.PI * z );

                if( z == 0.0 )
                {   // log singularity
                    return double.NaN;
                }

                z = LOGPI - Math.Log( z ) - w;
                return sign*z;
            }

            if( x < 13.0 )
            {
                z = 1.0;
                p = 0.0;
                u = x;

                while( u >= 3.0 )
                {
                    p--;
                    u = x + p;
                    z *= u;
                }

                while( u < 2.0 )
                {
                    if( u == 0.0 ) return double.NaN;
                    z /= u;
                    p++;
                    u = x + p;
                }

                if( z < 0.0 )
                {
                    lsign = -1;
                    z = -z;
                }
                else
                    lsign = 1;

                sign=lsign;

                if( u == 2.0 )
                    return( Math.Log(z) );

                p -= 2.0;
                x = x + p;
                p = x * Algebra.PolyEval( x, B) / Algebra.PolyEval( x, C);

                return( Math.Log(z) + p );
            }

            if( x > 2.556348e305 )
            {
                return sign * double.PositiveInfinity;
            }

            q = ( x - 0.5 ) * Math.Log(x) - x + LS2PI;
            if( x > 1.0e8 )
                return sign*q;

            p = 1.0/(x*x);

            if( x >= 1000.0 )
            {
                q += (( 7.9365079365079365079365e-4 * p
                - 2.7777777777777777777778e-3) *p
                + 0.0833333333333333333333) / x;
            }
            else
                q += Algebra.PolyEval( p, A ) / x;

            return sign*q ;
        }

        public static double Psi(double x)
        {
            List<double> A = new List<double>
            {
                8.33333333333333333333E-2,
                -2.10927960927960927961E-2,
                7.57575757575757575758E-3,
                -4.16666666666666666667E-3,
                3.96825396825396825397E-3,
                -8.33333333333333333333E-3,
                8.33333333333333333333E-2
            };

            double p, q, s, w, y, z;
            int i, n;

            bool negative = false;
            double nz = 0.0;

            if( x <= 0.0 )
            {
                negative = true;
                q = x;
                p = Math.Floor(q);
                if( p == q )
                {
                  // throw exception"Psi SINGULARITY");
                    return double.MaxValue;
                }

                /* Remove the zeros of tan(PI x)
                 * by subtracting the nearest integer from x
                 */

                nz = q - p;
                if( nz != 0.5 )
                {
                    if( nz > 0.5 )
                    {
                        p += 1.0;
                        nz = q - p;
                    }
                    nz = Math.PI/Math.Tan(Math.PI*nz);
                }
                else
                {
                    nz = 0.0;
                }
                x = 1.0 - x;
            }

           /* check for positive integer up to 10 */
            if( (x <= 10.0) && (x == Math.Floor(x)) )
            {
                y = 0.0;
                n = (int)x;
                for( i=1; i<n; i++ )
                {
                  w = i;
                  y += 1.0/w;
                }
                y -= EUL;

                if(negative) y-=nz;
                return y;
            }

            s = x;
            w = 0.0;
            while( s < 10.0 )
            {
                w += 1.0/s;
                s += 1.0;
            }

            if( s < 1.0e17 )
            {
                z = 1.0/(s * s);
                y = z * Algebra.PolyEval( z, A);
            }
            else
            {
                y = 0.0;
            }

            y = Math.Log(s)  -  (0.5/s)  -  y  -  w;

            if( negative )
            {
                y -= nz;
            }

            return y;
        }

        public static bool TestLogGamma()
        {
            List<double> x = new List<double> { 
                -1E6,
                -35.1,
                -31,
                -1.5,
                1,
                0,
                0.5,
                1.5,
                15.75, 
                35,
                121,
                double.PositiveInfinity
            };
            bool result = true;
            for (int i = 0; i < x.Count; i++)
            {
                double y = LogGamma(x[i]);
            }
            return result;
        }

        public static bool TestPsi()
        {
            List<double> x = new List<double> { 
                0.5,  // -log2 - gamma;
                1,    // -gamma
                Math.PI,
                Math.E, 
                10 
            };
            bool result = true;
            for (int i = 0; i < x.Count; i++)
            {
                double y = Psi(x[i]);
            }
            return result;
        }

        public static double Trigamma(double x)
        {
            // poor man's trigamma:
            double epsilon = 1E-3;
            return (Psi(x + epsilon) - Psi(x))/epsilon;
        }

        private static double EUL = 0.57721566490153286061;
    }

    class Algebra
    {
        public Algebra()
        {

        }

        public static double PolyEval(double x, List<double> coef)
        {
            double result = 0;
            int n = coef.Count()-1;
            for (int i = n; i >= 0; i--)
            {
                result = result * x + coef[i];
            }
            return result;
        }

        public static bool TestPolyEval()
        {
            List<double> coef = new List<double> { 1, 2, 3, 4 };
            double x = Math.PI;
            double y = PolyEval(x, coef);
            return (Math.Abs(y-160.9171052)<1E-7);
        }
    }
}
