using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Troschuetz.Random;
using System.IO;

using System.Security;

namespace TBKMath
{
    public struct MCMCProposal
    {
        // theta is the newly proposed value for the parameter array
        public double[] Theta;
        // LogRatio is the log of the ratio Pr(oldTheta|Theta)/Pr(Theta|oldTheta)
        public double LogRatio;
    }

    public struct MCMCState
    {
        public double[] Theta;
        public double LogLikelihood;
        public double LogPrior;
        public bool lastAccepted;
    }

    public abstract class MCMCProposalGenerator
    {
        public abstract MCMCProposal Proposal(double[] theta);
    }

    public class SimpleSimultaneousGaussianProposal : MCMCProposalGenerator
    {
        Troschuetz.Random.Generators.MT19937Generator gen;
        Troschuetz.Random.Distributions.Continuous.NormalDistribution gDist;
        double[] sigma;
        int dim;

        public SimpleSimultaneousGaussianProposal(double[] sigma)
        {
            this.sigma = sigma;
            gen = new Troschuetz.Random.Generators.MT19937Generator();
            gDist = new Troschuetz.Random.Distributions.Continuous.NormalDistribution(gen, 0, 1);
            dim = sigma.Length;
        }

        public override MCMCProposal Proposal(double[] theta)
        {
            if (theta.Length != dim)
            {
                throw new ArgumentException("The length of the parameter vector is inconsistent with previous lengths.");
            }
            double[] newTheta = new double[dim];
            for (int i = 0; i < dim; i++)
            {
                newTheta[i] = theta[i] + sigma[i] * gDist.NextDouble();
            }

            return new MCMCProposal() { Theta = newTheta, LogRatio = 0 };
        }
    }

    public class SimpleSequentialGaussianProposal : MCMCProposalGenerator
    {
        Troschuetz.Random.Generators.MT19937Generator gen;
        Troschuetz.Random.Distributions.Continuous.NormalDistribution gDist;
        Troschuetz.Random.Distributions.Discrete.DiscreteUniformDistribution duDist;
        double[] sigma;
        int dim;

        public SimpleSequentialGaussianProposal(double[] sigma)
        {
            this.sigma = sigma;
            gen = new Troschuetz.Random.Generators.MT19937Generator();
            gDist = new Troschuetz.Random.Distributions.Continuous.NormalDistribution(gen, 0, 1);
            dim = sigma.Length;
            duDist = new Troschuetz.Random.Distributions.Discrete.DiscreteUniformDistribution(gen, 0, dim - 1);
        }

        public override MCMCProposal Proposal(double[] theta)
        {
            if (theta.Length != dim)
            {
                throw new ArgumentException("The length of the parameter vector is inconsistent with previous lengths.");
            }
            double[] newTheta = theta.Clone() as double[];
            int i = duDist.Next();
            newTheta[i] = theta[i] + sigma[i] * gDist.NextDouble();

            return new MCMCProposal() { Theta = newTheta, LogRatio = 0 };
        }
    }

    public class ParallelHierarchicalSampler
    {
        // the dimension of the full parameter space
        private int dim;
        // the number of auxiliary chains
        private int numAuxiliary;
        // the collection of auxiliary chains
        public AuxiliaryChain[] mcmc;
        // the mother state
        public MCMCState Mother;
        // the index of the last auxiliary that was swapped
        private int lastAuxChain;

        Troschuetz.Random.Generators.MT19937Generator gen;
        Troschuetz.Random.Distributions.Discrete.DiscreteUniformDistribution discreteUniformDist;

        public delegate double LikelihoodDelegate(double[] theta);
        public delegate double LogPriorDelegate(double[] theta);
        public delegate MCMCProposal ProposalDelegate(double[] theta);
        private LikelihoodDelegate LLD;
        private LogPriorDelegate LPD;

        public int burnIn;
        public int total;
        double LLMean;
        public double[] mean;
        public double[,] covariance;
        public double[] CI;
        public string HistoryFile;
        public struct datum
        {
            public double[] x;
            public double LL;
        }
        public List<datum> History;
        public bool SkipCovarianceComputation = false;
        public int SamplingInterval = 10;
        public string OutputDirectory;

        public ParallelHierarchicalSampler(int _total, int _burnIn, int _numSubSpaces, int _dim)
        {
            OutputDirectory = Directory.GetCurrentDirectory();

            numAuxiliary = _numSubSpaces;
            dim = _dim;
            total = _total;
            burnIn = _burnIn;
            mean = new double[dim];
            if (!SkipCovarianceComputation)
            {
                covariance = new double[dim, dim];
            }
            CI = new double[dim];

            mcmc = new AuxiliaryChain[numAuxiliary];
            for (int i = 0; i < numAuxiliary; i++)
            {
                mcmc[i] = new AuxiliaryChain();
            }

            gen = new Troschuetz.Random.Generators.MT19937Generator();
            discreteUniformDist = new Troschuetz.Random.Distributions.Discrete.DiscreteUniformDistribution(gen, 0, numAuxiliary - 1);
        }

        public void Initialize(LikelihoodDelegate _LLD, LogPriorDelegate _LPD, double[] _Theta)
        {
            LLD = _LLD;
            LPD = _LPD;
            Mother.Theta = _Theta;
            Mother.LogPrior = LPD(Mother.Theta);
            Mother.LogLikelihood = LLD(Mother.Theta);
        }

        public double[] Run()
        {
            mean = new double[dim];
            if (!SkipCovarianceComputation)
            {
                covariance = new double[dim, dim];
            }

            if (HistoryFile == null) HistoryFile = OutputDirectory + @"\MCMCHistory.txt";
            File.Delete(HistoryFile);
            FileInfo fi = new FileInfo(HistoryFile);
            StreamWriter sw = fi.AppendText();

            string ThetaFile = OutputDirectory + @"\Theta.txt";
            StreamWriter tw = new FileInfo(ThetaFile).CreateText();

            for (int i = 0; i < burnIn; i++)
            {
                Step();
            }
            for (int i = 0; i < total; i++)
            {
                Step();
                stepStatistics(Mother.Theta);
                LLMean += Mother.LogLikelihood;
                if (i % SamplingInterval == 0)
                {
                    writeHistory(i, sw, Mother);
                    writeTheta(tw, Mother.Theta);
                }
            }

            finishStatistics();
            LLMean /= total;
            writeStatistics(OutputDirectory);
            tw.Flush();
            tw.Close();
            sw.Flush();
            sw.Close();
            return Mother.Theta;
        }

        public void Step()
        {
            lastAuxChain = discreteUniformDist.Next();
            Mother = mcmc[lastAuxChain].SwapStates(Mother);

            for (int i = 0; i < numAuxiliary; i++)
            {
                if (i == lastAuxChain) continue;  // skip the swapped chain
                mcmc[i].Step();
            }
        }

        private void writeHistory(int step, StreamWriter sw, MCMCState state)
        {
            sw.Write(step + "\t" + state.LogLikelihood.ToString());
            sw.Write("\t" + lastAuxChain.ToString());
            sw.WriteLine("\t" + state.lastAccepted.ToString());
            sw.Flush();
        }

        private void stepStatistics(double[] theta)
        {
            for (int i = 0; i < dim; i++)
            {
                mean[i] += theta[i];
            }

            if (!SkipCovarianceComputation)
            {
                for (int i = 0; i < dim; i++)
                {
                    for (int j = 0; j < dim; j++)
                    {
                        covariance[i, j] += theta[i] * theta[j];
                    }
                }
            }

        }

        private void finishStatistics()
        {
            for (int i = 0; i < dim; i++)
            {
                mean[i] /= total;
            }

            if (!SkipCovarianceComputation)
            {
                for (int i = 0; i < dim; i++)
                {
                    for (int j = 0; j < dim; j++)
                    {
                        covariance[i, j] -= total * mean[i] * mean[j];
                        covariance[i, j] /= total - 1;
                    }
                }
            }
        }

        private void writeStatistics(string path)
        {
            // open file for writing, ensuring that the process does not crash the program
            int count = 0;
            bool success = false;
            string fileName = path + @"\MCMCStatistics.txt";
            StreamWriter sw = null;
            while (!success)
            {
                try
                {
                    FileInfo fi = new FileInfo(fileName);
                    sw = fi.CreateText();
                    success = true;
                }
                catch
                {
                    fileName = path + @"\MCMCStatistics" + count.ToString("D3") + ".txt";
                }
            }

            sw.Write(mean[0]);
            for (int i = 1; i < dim; i++)
            {
                sw.Write("\t" + mean[i].ToString());
            }
            sw.WriteLine();

            if (!SkipCovarianceComputation)
            {
                for (int i = 0; i < dim; i++)
                {
                    sw.Write(covariance[i, 0].ToString());
                    for (int j = 1; j < dim; j++)
                    {
                        sw.Write("\t" + covariance[i, j].ToString());
                    }
                    sw.WriteLine();
                }
            }
            sw.Close();
        }

        private void writeTheta(StreamWriter sw, double[] theta)
        {
            for (int i = 0; i < theta.Length; i++)
            {
                sw.Write(theta[i] + "\t");
            }
            sw.Write("\n");
        }
    }

    public class AuxiliaryChain
    {
        private bool lastAccepted;
        public int accepted;
        public bool AcceptAll = false;

        // These may be the same as the PHS class 
        public delegate double LikelihoodDelegate(double[] theta);
        public delegate double LogPriorDelegate(double[] theta);
        public delegate MCMCProposal ProposalDelegate(double[] theta);

        private LikelihoodDelegate LD;
        private LogPriorDelegate LPD;
        private ProposalDelegate PD;

        // the state of the full system
        private MCMCState state;
        // the size of the subspace parameter set
        private int dim;
        // the subspace parameter set
        private double[] Theta;
        // indices that mape the subspace parameters to the full system parameters
        private int[] indices;

        private Troschuetz.Random.Generators.MT19937Generator mt;
        private Troschuetz.Random.Distributions.Continuous.ContinuousUniformDistribution uRand;

        public AuxiliaryChain()
        {
            mt = new Troschuetz.Random.Generators.MT19937Generator();
            uRand = new Troschuetz.Random.Distributions.Continuous.ContinuousUniformDistribution(mt, 0, 1);
        }

        /// <summary>
        /// Update the auxiliary chain working parameter set from the full parameter set
        /// </summary>
        private void updateWorkingFromFullParams()
        {
            for (int i = 0; i < dim; i++)
            {
                Theta[i] = state.Theta[indices[i]];
            }
        }
        /// <summary>
        /// Update the full parameter set from the auxiliary chain working parameter set
        /// </summary>
        private void updateFullFromWorkingParams()
        {
            for (int i = 0; i < dim; i++)
            {
                state.Theta[indices[i]] = Theta[i];
            }
        }
        /// <summary>
        /// Create a copy of the full state and update it with the new working parameters
        /// </summary>
        /// <param name="p">working parameters</param>
        /// <returns></returns>
        private double[] updateCopyFullParams(double[] p)
        {
            double[] stateCopy = (double[])(state.Theta).Clone();
            for (int i = 0; i < dim; i++)
            {
                stateCopy[indices[i]] = p[i];
            }
            return stateCopy;
        }

        public MCMCState SwapStates(MCMCState _motherState)
        {
            // preserve the current set of full parameters for returning to the parent
            MCMCState auxiliaryState = state;
            state = _motherState;
            // update the working parameters
            updateWorkingFromFullParams();
            return auxiliaryState;
        }

        public void Initialize(LikelihoodDelegate _LD, LogPriorDelegate _LPD, ProposalDelegate _PD, int[] _indices, MCMCState _state)
        {
            LD = _LD;
            LPD = _LPD;
            PD = _PD;
            accepted = 0;
            state = _state;           
            dim = _indices.Length;
            indices = new int[dim];
            indices = _indices;
            Theta = new double[dim];
            updateWorkingFromFullParams();
        }

        /// <summary>
        /// generates a single step in an MCMC algorithm
        /// </summary>
        public void Step()
        {
            lastAccepted = false;
            state.lastAccepted = false;

            // the argument is the likelihood value at the current position
            if (LD == null) return;

            // generate a new point in the auxiliary chain working parameter space
            MCMCProposal mcmcp = PD(Theta);
            // Create a new full state that is up-to-date with the proposed parameters
            MCMCState newState = new MCMCState();
            newState.Theta = updateCopyFullParams(mcmcp.Theta);

            // compute likelihood and prior at new point
            newState.LogLikelihood = LD(newState.Theta);
            newState.LogPrior = LPD(newState.Theta);

            // decide whether to accept the new point
            if ((newState.LogLikelihood + newState.LogPrior > state.LogLikelihood + state.LogPrior) | AcceptAll)
            {
                lastAccepted = true;
                accepted++;
            }
            else
            {
                double U = uRand.NextDouble();
                if (Math.Log(U) < newState.LogLikelihood + newState.LogPrior - (state.LogLikelihood + state.LogPrior))
                {
                    lastAccepted = true;
                    accepted++;
                }
                else state.lastAccepted = false;
            }

            if (lastAccepted)
            {
                newState.lastAccepted = true;
                // update the working parameters
                Theta = mcmcp.Theta;
                // update the full set of parameters
                state = newState;
            }
        }
    }

    public class MCMC
    {
        public int burnIn;
        public int total;

        private bool lastAccepted;
        public int accepted;

        private int dim;

        double LLMean;
        public double[] mean;
        public double[,] covariance;

        public double[] CI;

        public string HistoryFile;

        public double[] ComponentScale;
        public double[] FinalScale;

        private Troschuetz.Random.Generators.MT19937Generator mt;
        private Troschuetz.Random.Distributions.Continuous.ContinuousUniformDistribution uRand;

        public delegate double LikelihoodDelegate(double[] theta);
        public delegate double LogPriorDelegate(double[] theta);
        public delegate MCMCProposal ProposalDelegate(double[] theta);

        private LikelihoodDelegate LD;
        private LogPriorDelegate LPD;
        private ProposalDelegate PD;

        double LLV;
        double LPV;
        double[] Theta;

        public struct datum
        {
            public double[] x;
            public double LL;
        }
        public List<datum> History;

        public bool SkipCovarianceComputation = false;
        public int SamplingInterval = 1;
        public string OutputDirectory;

        public bool AcceptAll = false;

        public MCMC()
        {
            mt = new Troschuetz.Random.Generators.MT19937Generator();
            uRand = new Troschuetz.Random.Distributions.Continuous.ContinuousUniformDistribution(mt, 0, 1);
            OutputDirectory = Directory.GetCurrentDirectory();
        }

        public double[] Swap(double[] _Theta)                               
        {
            double[] oldTheta = Theta;
            Theta = _Theta;
            return oldTheta;
        }

        public void Initialize(LikelihoodDelegate _LD, LogPriorDelegate _LPD, ProposalDelegate _PD, int dim, int _total, int _burnIn)
        {
            LD = _LD;
            LPD = _LPD;
            PD = _PD;
            total = _total;
            burnIn = _burnIn;
            accepted = 0;
            this.dim = dim;

            mean = new double[dim];
            if (!SkipCovarianceComputation)
            {
                covariance = new double[dim, dim];
            }

            CI = new double[3];
        }

        private double MCMCStep(double LLV, double LPV, ref double[] theta)
        {
            // the argument is the likelihood value at the current position
            if (LD == null) return Double.NaN;

            // generates a single step in an MCMC algorithm
            lastAccepted = false;

            // generate new point in parameter space
            MCMCProposal mcmcp = PD(theta);

            double[] thetaNew = mcmcp.Theta;

            // compute likelihood and prior at new point
            double LLVNew = LD(thetaNew);
            double LPVNew = LPD(thetaNew);

            double logAccept = LLVNew + LPVNew - (LLV + LPV) + mcmcp.LogRatio;

            // decide whether to accept new point
            if ((LLVNew + LPVNew > LLV + LPV) | AcceptAll)
            {
                lastAccepted = true;
                accepted++;
            }
            else
            {
                // debug:
                double U = uRand.NextDouble();
                //////////////////////////////
                if (Math.Log(U) < LLVNew + LPVNew - (LLV + LPV))
                {
                    lastAccepted = true;
                    accepted++;
                }
                else lastAccepted = false;
            }
            if (lastAccepted)
            {
                theta = thetaNew;
                return LLVNew;
            }
            return LLV;
        }

        public void InitializeRun(double[] theta)
        {
            if (HistoryFile == null) HistoryFile = OutputDirectory + @"\MCMCHistory.txt";
            File.Delete(HistoryFile);

            accepted = 0;
            double logLik = LD(theta);
            double logPrior = LPD(theta);

            dim = theta.Length;
            mean = new double[dim];

        }

        public void Step(double logLik, double logPrior, double[] theta)
        {
            logLik = MCMCStep(logLik, logPrior, ref theta);
        }

        public double[] MCMCRun(double[] theta)
        {
            if (HistoryFile == null) HistoryFile = OutputDirectory + @"\MCMCHistory.txt";
            File.Delete(HistoryFile);

            accepted = 0;
            double logLik = LD(theta);
            double logPrior = LPD(theta);

            dim = theta.Length;
            mean = new double[dim];
            if (!SkipCovarianceComputation)
            {
                covariance = new double[dim, dim];
            }

            FileInfo fi = new FileInfo(HistoryFile);
            StreamWriter sw = fi.AppendText();

            for (int i = 0; i < burnIn; i++)
            {
                logLik = MCMCStep(logLik, logPrior, ref theta);
                // WriteHistory(sw, logLik, theta);
            }
            for (int i = 0; i < total; i++)
            {
                logLik = MCMCStep(logLik, logPrior, ref theta);
                stepStatistics(theta);
                LLMean += logLik;
                if (i % SamplingInterval == 0)
                {
                    WriteHistory(i, sw, logLik);
                    string ThetaFile = OutputDirectory + @"\Theta(" + i + ").txt";
                    using (StreamWriter tw = new FileInfo(ThetaFile).CreateText())
                    {
                        writeTheta(tw, theta);
                    }
                }
            }
            finishStatistics();
            LLMean /= total;
            writeStatistics(OutputDirectory);
            sw.Close();
            return theta;
        }

        private void WriteHistory(int step, StreamWriter sw, double logLik)
        {
            sw.Write(step + "\t" + logLik.ToString());
            sw.WriteLine("\t" + (lastAccepted ? 1 : 0).ToString());
            sw.Flush();
        }

        private void writeTheta(StreamWriter sw, double[] theta)
        {
            for (int i = 0; i < theta.Length; i++)
            {
                sw.WriteLine(i + "\t" + theta[i]);
            }
        }

        public void ConfInt(List<double> iTarg)
        {
            //List<double> c = new List<double>();

            //// initialize guess
            //double cTarg = a.xi - Math.Log(2);

            //accepted = 0;
            //double logLik = a.logLikelihood(new double[3] { a.alpha, a.beta, a.xi }, a.tau, cTarg, iTarg);

            //// during burn-in, change only the concentration
            //double save_searchRadius = this.searchRadius;
            //this.IntializeSDs(0);
            //this.sdCTarg = 2;
            //for (int i = 0; i < burnIn; i++)
            //{
            //    logLik = MCMCStep(logLik, ref cTarg, iTarg);
            //}

            //accepted = 0;
            //this.IntializeSDs(save_searchRadius);
            //for (int i = 0; i < total; i++)
            //{
            //    logLik = MCMCStep(logLik, ref cTarg, iTarg);
            //    c.Add(cTarg);
            //}

            //c.Sort();
            //CI[0] = c[(int)(0.05 * total)];
            //CI[1] = c[(int)(0.5 * total)];
            //CI[2] = c[(int)(0.95 * total)];
        }

        private void stepStatistics(double[] theta)
        {
            for (int i = 0; i < dim; i++)
            {
                mean[i] += theta[i];
            }

            if (!SkipCovarianceComputation)
            {
                for (int i = 0; i < dim; i++)
                {
                    for (int j = 0; j < dim; j++)
                    {
                        covariance[i, j] += theta[i] * theta[j];
                    }
                }
            }

        }

        private void finishStatistics()
        {
            for (int i = 0; i < dim; i++)
            {
                mean[i] /= total;
            }

            if (!SkipCovarianceComputation)
            {
                for (int i = 0; i < dim; i++)
                {
                    for (int j = 0; j < dim; j++)
                    {
                        covariance[i, j] -= total * mean[i] * mean[j];
                        covariance[i, j] /= total - 1;
                    }
                }
            }
        }

        private void writeStatistics(string path)
        {
            // open file for writing, ensuring that the process does not crash the program
            int count = 0;
            bool success = false;
            string fileName = path + @"\MCMCStatistics.txt";
            StreamWriter sw = null;
            while (!success)
            {
                try
                {
                    FileInfo fi = new FileInfo(fileName);
                    sw = fi.CreateText();
                    success = true;
                }
                catch
                {
                    fileName = path + @"\MCMCStatistics" + count.ToString("D3") + ".txt";
                }
            }

            sw.Write(mean[0]);
            for (int i = 1; i < dim; i++)
            {
                sw.Write("\t" + mean[i].ToString());
            }
            sw.WriteLine();

            if (!SkipCovarianceComputation)
            {
                for (int i = 0; i < dim; i++)
                {
                    sw.Write(covariance[i, 0].ToString());
                    for (int j = 1; j < dim; j++)
                    {
                        sw.Write("\t" + covariance[i, j].ToString());
                    }
                    sw.WriteLine();
                }
            }
            sw.Close();
        }

    }
}
