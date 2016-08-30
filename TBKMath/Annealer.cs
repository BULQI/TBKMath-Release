using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Troschuetz.Random;
using System.IO;

namespace TBKMath
{
    public class Annealer
    {
        public int total;
        private bool lastAccepted;
        public int accepted;
        private int dim;
        double LLMean;

        private double searchRadius;
        public double SearchRadius
        {
            get { return this.searchRadius; }
        }

        public double Temperature;
        public string HistoryFile;
        public double[] Scale;
        private double[] scale;

        private Troschuetz.Random.Generators.MT19937Generator mt;
        private Troschuetz.Random.Distributions.Continuous.ContinuousUniformDistribution uRand;
        private Troschuetz.Random.Distributions.Continuous.NormalDistribution zRand;

        public delegate double ObjectiveFunctionDelegate(double[] theta);
        private ObjectiveFunctionDelegate ofd;

        public Annealer()
        {
            mt = new Troschuetz.Random.Generators.MT19937Generator();
            uRand = new Troschuetz.Random.Distributions.Continuous.ContinuousUniformDistribution(mt);
            zRand = new Troschuetz.Random.Distributions.Continuous.NormalDistribution(mt);
        }

        public void Initialize(ObjectiveFunctionDelegate _ofd, int _total, double[] _scale, bool[] _fixt)
        {
            ofd = _ofd;
            total = _total;
            accepted = 0;
            dim = _scale.Length;
            Scale = _scale;

            searchRadius = 1.0 / Scale.Length;

            this.IntializeScale(searchRadius, _fixt);

            // uniform on [0,1]
            uRand.Alpha = 0.0;
            uRand.Beta = 1.0;

            // standard gaussian parameters
            zRand.Mu = 0;
            zRand.Sigma = 1;

            Temperature = 1;
        }

        public void IntializeScale(double _searchRadius, bool[] fixt)
        {
            scale = new double[fixt.Length];
            for (int i = 0; i < dim; i++)
            {
                scale[i] = fixt[i] ? 0 : Scale[i] * _searchRadius;
            }
        }

        private double AnnealStep(double objectiveValue,  ref double[] theta)
        {
            // the argument is the function value at the current position
            if (ofd == null) return Double.NaN;

            // generates a single step in the optimization
            lastAccepted = false;

            // generate new point in parameter space
            double[] thetaNew = theta.Clone() as double[];
            for (int i = 0; i < scale.Length; i++)
            {
                thetaNew[i] += scale[i] * zRand.NextDouble();
            }
            
            // compute function at new point
            double newObjectiveValue = ofd( thetaNew);

            // decide whether to accept new point
            if (newObjectiveValue > objectiveValue)
            {
                lastAccepted = true;
                accepted++;
            }
            else if (Temperature > 0)
            {
                double U = uRand.NextDouble();
                if (Temperature * Math.Log(U) <= newObjectiveValue - objectiveValue)
                {
                    lastAccepted = true;
                    accepted++;
                }
                else lastAccepted = false;
            }
            else lastAccepted = false;

            if (lastAccepted)
            {
                theta = thetaNew;
                return newObjectiveValue;
            }
            return objectiveValue;
        }

        public void Anneal(double[] theta)
        {
            if (HistoryFile == null) HistoryFile = "AnnealHistory.txt";

            accepted = 0;
            double objective = ofd( theta);

            dim = theta.Length;

            FileInfo fi = new FileInfo(HistoryFile);
            StreamWriter sw = fi.AppendText();
            
            for (int i = 0; i < total; i++)
            {
                objective = AnnealStep(objective,  ref theta);
                WriteHistory(sw, objective, theta);
            }
            LLMean /= total;
            sw.Close();
        }

        private void WriteHistory(StreamWriter sw, double logLik, double[] theta)
        {
            sw.Write(logLik.ToString());
            foreach (double t in theta)
            {
                sw.Write("\t" + t.ToString());
            }
            sw.WriteLine("\t" + (lastAccepted ? 1 : 0).ToString());
            sw.Flush();
        }
    }
}
