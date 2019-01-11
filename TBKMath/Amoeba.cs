using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBKMath
{

    /// <summary>
    /// Performs Amoeba minimization
    /// </summary>
    public class Amoeba : EstimationProcess
    {
        /*    Translated into c# from
              W.H.Press, B.P.Flannery, S.A. Teukolsky, W.T. Vetterling
              Numerical Recipes: the art of scientific computing
              1986 Cambridge University Press

              Modified and documented by TBK
        
            Notes:
              Point must be set before proceeding with the minimization 
              Convergence criterion is
         
              errorEst < ftol

              where errorEst = | f_Min - f_Max | /  ( | f_Min | + | f_Max | ) / 2

        */

        public double ToleranceAchieved;
        public double ToleranceRequested;
        public double DefaultEdgeSize;

        private int nVertices; // equals ndim + 1
        private int ndim;
        public bool ForceVertexCreation { get; set; }

        double fmin;

        public List<double> FunctionValues;

        public List<List<double>> Vertices;

        List<List<double>> p;

        public Amoeba()
        {

        }

        public override void Start()
        {
            if (function == null) { } // throw exception
            ndim = Point.Count;
            nVertices = ndim + 1;

            if (Scale == null)
            {
                if (DefaultEdgeSize == 0) DefaultEdgeSize = 1;
                MakeEdges();
            }
            // HS: allow recreating the vertices every time to ensure we start finding a new solution
            if (Vertices == null || ForceVertexCreation == true) MakeVertices();
            if (MaxNumIterations == 0) MaxNumIterations = 10000;

            if (verbose == true && HF == null)
            {
                string defaultFileName = "AmoebaHistory.txt";
                HF = new HistoryFile(defaultFileName);
            }
                        
            minimize();
            if (verbose == true)
            {
                HF.Close();
            }
        }

        private void minimize(/* ref List<List<double>> pp */ ) // pp -> Vertices
        {
            
            const double TINY=1.0e-10;
            int ihi,ilo,inhi;
            
            List<double> psum = new List<double>();
            List<double> pmin = new List<double>();

            p = new List<List<double>>(Vertices); // is a working copy of the simplex
            FunctionValues = new List<double>(); // values of the objective function at each vertex

            // construct the working simplex
            for (int i=0; i < nVertices; i++) 
            {
                List<double> x = new List<double>();
                for (int j = 0; j < ndim; j++)
                {
                    x.Add(p[i][j]);
                }
                double f = function(x);
                if (double.IsNaN(f)) f = double.MaxValue;
                FunctionValues.Add(f);
            }

            NumIterations=0;
            psum = get_psum(p);

            while(true)
            {

                ilo=0;
                if (FunctionValues[0] > FunctionValues[1])
                {
                    inhi = 1;
                    ihi = 0;
                }
                else
                {
                    inhi = 0;
                    ihi = 1;
                }

                for (int i = 0; i < nVertices; i++) 
                {
                    // find vertex with lowest function value
                    if (FunctionValues[i] <= FunctionValues[ilo]) ilo = i;

                    // find vertices with greatest (ihi) and next-greatest (inhi) function values
                    if (FunctionValues[i] > FunctionValues[ihi]) 
                    {
                        inhi = ihi;
                        ihi = i;
                    }
                    else if (FunctionValues[i] > FunctionValues[inhi] && i != ihi) inhi = i;
                }

                // see if we have achieved requested tolerance.  If so, make final adjustments and return
                ToleranceAchieved = 2.0 * Math.Abs(FunctionValues[ihi] - FunctionValues[ilo]) /
                    (Math.Abs(FunctionValues[ihi]) + Math.Abs(FunctionValues[ilo]) + TINY);

                if (ToleranceAchieved < ToleranceRequested)  
                {
                    Swap(0, ilo, FunctionValues);
                    for (int i=0; i < ndim; i++) 
                    {
                        Swap( p[0][i], p[ilo][i] );

                        pmin.Add(p[0][i]);
                    }

                    fmin = FunctionValues[0];
                    Point = pmin;
                    if (verbose == true)
                    {
                        HF.AppendData(fmin, Point);
                    }
                    return;
                }

                if (NumIterations >= MaxNumIterations) 
                {
                    // TO DO: throw exception
                    return;
                }

                NumIterations += 2; 

                // take the next step in the process
                double yTry = amoTry(ref psum, ihi, -1.0);
                if (yTry <= FunctionValues[ilo])
                {
                    // if it was successful, try again
                    yTry = amoTry(ref psum, ihi, 2.0);
                }

                else if (yTry >= FunctionValues[inhi]) 
                {
                    double ysave = FunctionValues[ihi];
                    yTry = amoTry(ref psum, ihi, 0.5);
                    if (yTry >= ysave) 
                    {
                        for (int i=0; i < nVertices; i++) 
                        {
                            if (i != ilo) 
                            {
                                for (int j=0;j<ndim;j++)
                                {
                                    p[i][j] = psum[j] = 0.5*( p[i][j] + p[ilo][j] );
                                }
                                double f = function(psum);
                                if (double.IsNaN(f)) f = double.MaxValue;
                                FunctionValues[i] = f;
                            }
                        }
                        NumIterations += ndim;
                        psum = get_psum(p);
                    }
                } else --NumIterations;

                if (verbose)
                {
                    HF.AppendData(FunctionValues[ilo], p[ilo]);
                }
            }
        }

        double amoTry(ref List<double> psum, int ihi, double fac)
        {
            // makes an elementary move in the algorithm
            // 
            // p try is the new point
            List<double> pTry = new List<double>();

            // the factors determine where p will be placed
            double fac1 = (1.0 - fac) / ndim;
            double fac2 = fac1 - fac;

            // place pTry
            for (int j = 0; j < ndim; j++)
            {
                pTry.Add(psum[j] * fac1 - p[ihi][j] * fac2);
            }

            // evaluate the objective function at pTry
            double f = function(pTry);
            if (double.IsNaN(f)) f = double.MaxValue;
            double ytry = f;

            // if successul (reduced the maximum value of the objective function on the simplex)
            // replace argMax with pTry
            if (ytry < FunctionValues[ihi])
            {
                FunctionValues[ihi] = ytry;
                for (int j = 0; j < ndim; j++)
                {
                    // update psum
                    psum[j] += pTry[j] - p[ihi][j];
                    // update Vertices
                    p[ihi][j] = pTry[j];
                }
            }
            // return the objective function value at the new vertex
            return ytry;
        }

        private List<double> get_psum(List<List<double>> _p)
        {
            // returns a vector 
            List<double> psum = new List<double>();
            for (int j=0; j < ndim; j++) 
            {
                double sum = 0.0;
                for (int i=0; i < nVertices; i++)
                {
                    sum += _p[i][j];
                }
                psum.Add(sum);
            }
            return psum;
        }

        public void MakeEdges()
        {
            Scale = new List<double>();
            for (int i = 0; i < Point.Count; i++) { Scale.Add(DefaultEdgeSize); }
        }

        public void MakeVertices()
        {
            // create vertices of the initial simplex
            // the matrix pp has ndim + 1 rows and ndim columns
            Vertices = new List<List<double>>();
            for (int i = 0; i < ndim + 1; i++)
            {
                List<double> row = new List<double>();
                for (int j = 0; j < ndim; j++)
                {
                    row.Add(Point[j]);
                }
                Vertices.Add(row);
                if (i != 0) Vertices[i][i - 1] += Scale[i - 1];
            }
        }

        public void Swap(double x, double y)
        {
            double temp = y;
            y = x;
            x = temp;
        }

        public void Swap(int index1, int index2, List<double> vector)
        {
            double temp = vector[index1];
            vector[index1] = vector[index2];
            vector[index2] = temp;
        }
    }
}
