using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TBKMath;
using Troschuetz.Random;
using MathNet.Numerics;

namespace DirichletProcessMCMC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private double GausLogLike(HashSet<double> block)
        {
            // hyperparameters
            double mu0 = 0;
            double kappa0 = 1;
            double nu0 = 1;
            double sigsq0 = 100;

            int n = block.Count;

            if (n == 0)
            {
                return 0;
            }

            double nun = nu0 + n;
            double kappan = kappa0 + n;

            double mean = 0;
            foreach (double number in block) { mean += number; }
            mean /= n;


            double ss = 0;
            foreach (double number in block)
            {
                ss += Math.Pow(number, 2);
            }
            ss -= n * mean * mean;

            double sigsqn = (nu0 * sigsq0 + ss + n * kappa0/kappan * Math.Pow(mu0 - mean, 2) )/nun;

            double loglik = MathNet.Numerics.SpecialFunctions.GammaLn(nun / 2) - MathNet.Numerics.SpecialFunctions.GammaLn(nu0 / 2);
            loglik += (Math.Log(kappa0) - Math.Log(kappan)) / 2;
            loglik += (nu0 * Math.Log(nu0 * sigsq0) - nun * Math.Log(nun * sigsqn)) / 2;
            loglik -= n * Math.Log(Math.PI) / 2;
            return loglik;
        }

        private double Gaus0LogLike(HashSet<double> block)
        {
            double mu = 0;
            double sigma = 1;

            int n = block.Count;

            if (n == 0)
            {
                return 0;
            }

            double loglik = -n / 2.0 * Math.Log(2 * Math.PI);

            double s2 = 0;
            foreach (double number in block)
            {
                s2 += Math.Pow(number, 2);
            }
            s2 = s2 - n * mu * mu;

            loglik -= n* Math.Log(sigma);
            loglik -= s2/Math.Pow(sigma,2)/2;
            return loglik;
        }

        private double Gaus1LogLike(HashSet<double> block)
        {
            double mu = 0;
            double sigma = 1;

            int n = block.Count;

            if (n == 0)
            {
                return 0;
            }

            double loglik = -n / 2.0 * Math.Log(2 * Math.PI);

            double s2 = 0;
            foreach (double number in block)
            {
                s2 += Math.Pow(number, 2);
            }
            s2 = s2 - n * mu * mu;

            loglik -= n * Math.Log(sigma);
            loglik -= s2 / Math.Pow(sigma, 2) / 2;
            return loglik;
        }

        private double dummyLik(HashSet<double> set)
        {
            return -Math.Log(1 + set.Count);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Troschuetz.Random.Distributions.Continuous.NormalDistribution rnorm = new Troschuetz.Random.Distributions.Continuous.NormalDistribution();
            int n = SizeBox.AsInt(10);
            double alpha = AlphaBox.AsDouble(1);
            double[] entities = GenerateProcess(n, alpha, 3, 1);

            // test likelihood
            StringBuilder s = new StringBuilder();
            HashSet<double> testset = new HashSet<double>();
            for (int i = 0; i < entities.Length; i++)
            {
                testset.Add(entities[i]);
                double lik = GausLogLike(testset);
                s.AppendLine(i + "\t" + lik);
            }
            // end test

            DPMCMC<double> dpmcmc = new DPMCMC<double>(entities, alpha, GausLogLike);
            dpmcmc.Initialize();
            string history = dpmcmc.Run(10000);
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            System.IO.File.WriteAllText(path + @"\DPMCMCHistory.txt", history);
        }

        private double[] GenerateProcess(int n, double alpha, double sigma1, double sigma2)
        {
            Troschuetz.Random.Distributions.Continuous.NormalDistribution rnorm1 = new Troschuetz.Random.Distributions.Continuous.NormalDistribution();
            rnorm1.Mu = 0;
            rnorm1.Sigma = sigma1;

            Troschuetz.Random.Distributions.Continuous.NormalDistribution rnorm2 = new Troschuetz.Random.Distributions.Continuous.NormalDistribution();
            rnorm2.Mu = 0;
            rnorm2.Sigma = sigma2;

            Troschuetz.Random.Distributions.Discrete.CategoricalDistribution rcat = new Troschuetz.Random.Distributions.Discrete.CategoricalDistribution();
            rcat.Weights = new List<double>() { 1 };
            List<double> p = new List<double>();
            List<double> mu = new List<double>();
            double[] x = new double[n];
            List<int> counts = new List<int>();

            int k = 0;
            for (int i = 0; i < n; i++)
            {
                int j = rcat.Next();
                if (j == k)
                {
                    mu.Add(rnorm1.NextDouble());
                    counts.Add(1);
                    k++;
                }
                else
                {
                    counts[j]++;
                }
                x[i] = rnorm2.NextDouble() + mu[j];
                List<double> weights = new List<double>();
                for (int k1 = 0; k1 < k; k1++)
                {
                    weights.Add(counts[k1] / (alpha + k));
                }
                weights.Add(alpha / (alpha + k));
                rcat.Weights = weights;
            }
            return x;
        }
    }
}
