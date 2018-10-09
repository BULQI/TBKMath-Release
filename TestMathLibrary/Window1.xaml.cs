using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace TestMathLibrary
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }

        private void dismissButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void testAmoebaButton_Click(object sender, RoutedEventArgs e)
        {
            TestAmoeba ta = new TestAmoeba();
            ta.runTest();
            MessageBox.Text += "Amoeba Test complete.\n"; 
            foreach (double p in ta.am.Point)
            {
                MessageBox.Text += p.ToString("E2") + "\n";
            }
            MessageBox.Text += " tolerance achieved = " + ta.am.ToleranceAchieved.ToString("E2") + "\n";
            MessageBox.Text += " function evaluations = " + ta.am.NumIterations.ToString() + "\n";
        }

        private void bisectButton_Click(object sender, RoutedEventArgs e)
        {
            TestBisector tb = new TestBisector();
            tb.runTest();
            MessageBox.Text += tb.argMin.ToString() + "\n";
            MessageBox.Text += "Bisector Test Complete.\n";
        }

        private double flatScores(HashSet<int> argument)
        {
            return 0;
        }

        private double parityScore(HashSet<int> argument)
        {
            if (argument.Count == 0)
                return 0;

            double mean = 0;
            foreach (int n in argument)
            {
                mean += n % 2;
            }
            mean /= argument.Count;

            double score = 1;
            foreach (int n in argument)
            {
                score += Math.Abs(n % 2 - mean) ;
            }

            return -score;
        }

        //private void mcmcButton_Click(object sender, RoutedEventArgs e)
        //{
        //    // initialize rng
        //    Troschuetz.Random.Generators.MT19937Generator mt = new Troschuetz.Random.Generators.MT19937Generator();
        //    Troschuetz.Random.Distributions.Continuous.NormalDistribution z = new Troschuetz.Random.Distributions.Continuous.NormalDistribution(mt, 0, 1);
        //    // make data
        //    double alpha = 0;
        //    double beta = 0.333;
        //    int n = 10;
        //    double tau = 0.1;
        //    double[] x = new double[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        //    double[] y = new double[n];
        //    for (int i = 0; i < n; i++)
        //    {
        //        y[i] = alpha + beta * x[i] + Math.Sqrt(tau) * z.NextDouble();
        //    }

        //    List<double> estimates = linearRegression(x, y);

        //    TestStatistics ts = new TestStatistics();
        //    ts.LoadData(x, y);

        //    MCMC mcmc = new MCMC();
        //    //SimpleSequentialGaussianProposal ssgp = new SimpleSequentialGaussianProposal(new double[] { 0.1, 0.1, 0.1 });
        //    //SimpleSimultaneousGaussianProposal sgp = new SimpleSimultaneousGaussianProposal(new double[] { 0.01, 0.01, 0.01 });
        //    SimpleSimultaneousGaussianProposal sgp = new SimpleSimultaneousGaussianProposal(new double[] { 0.01, 0.01, 0.01 });
        //    int burnin = 0;
        //    int total = 1000;
        //    mcmc.Initialize(ts.LogLikelihoodLR, ts.LogPriorLR, sgp.Proposal, 3, total, burnin);
        //    double tauInit = Math.Log(estimates[2]);
        //    double[] pars = new double[]{estimates[0],estimates[1],tauInit};
        //    mcmc.MCMCRun(pars);
        //}

        private void mcmcButton_Click(object sender, RoutedEventArgs e)
        {
            // initialize rng
            Troschuetz.Random.Generators.MT19937Generator mt = new Troschuetz.Random.Generators.MT19937Generator();
            Troschuetz.Random.Distributions.Continuous.NormalDistribution z = new Troschuetz.Random.Distributions.Continuous.NormalDistribution(mt, 0, 1);
            // make data
            double alpha = 0;
            double beta = 0.333;
            int n = 10;
            double tau = 0.1;
            double[] x = new double[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            double[] y = new double[n];
            for (int i = 0; i < n; i++)
            {
                y[i] = alpha + beta * x[i] + Math.Sqrt(tau) * z.NextDouble();
            }

            List<double> estimates = linearRegression(x, y);

            TestStatistics ts = new TestStatistics();
            ts.LoadData(x, y);

            double tauInit = Math.Log(estimates[2]);
            double[] pars = new double[] { estimates[0], estimates[1], tauInit };

            int burnin = 50000;
            int total = 50000;

            // 1 parameter per auxiliary chain
            int numSubModels = 3;
            AuxiliaryChain[] submodel = new AuxiliaryChain[] { new AuxiliaryChain(), new AuxiliaryChain(), new AuxiliaryChain() };
            SimpleSimultaneousGaussianProposal[] gp = new SimpleSimultaneousGaussianProposal[] 
                                        {   new SimpleSimultaneousGaussianProposal(new double[] { 0.01 }), 
                                            new SimpleSimultaneousGaussianProposal(new double[] { 0.01 }),
                                            new SimpleSimultaneousGaussianProposal(new double[] { 0.01  }) };
            ParallelHierarchicalSampler mother = new ParallelHierarchicalSampler(total, burnin, numSubModels, 3);
            mother.Initialize(ts.LogLikelihoodLR, ts.LogPriorLR, pars);
            for (int i = 0; i < numSubModels; i++)
            {
                mother.mcmc[i].Initialize(ts.LogLikelihoodLR, ts.LogPriorLR, gp[i].Proposal, new int[] { i }, mother.Mother);
            }

            ////// all 3 parameters in the auxiliary chains
            //int numSubModels = 3;
            //AuxiliaryChain[] submodel = new AuxiliaryChain[] { new AuxiliaryChain(), new AuxiliaryChain(), new AuxiliaryChain() };
            //SimpleSimultaneousGaussianProposal[] gp = new SimpleSimultaneousGaussianProposal[] 
            //                            {   new SimpleSimultaneousGaussianProposal(new double[] { 0.01, 0.01, 0.01 }), 
            //                                new SimpleSimultaneousGaussianProposal(new double[] { 0.01, 0.01, 0.01 }),
            //                                new SimpleSimultaneousGaussianProposal(new double[] { 0.01, 0.01, 0.01 }) };
            //ParallelHierarchicalSampler mother = new ParallelHierarchicalSampler(total, burnin, numSubModels, 3);
            //mother.Initialize(ts.LogLikelihoodLR, ts.LogPriorLR, pars);
            //for (int i = 0; i < numSubModels; i++)
            //{
            //    mother.mcmc[i].Initialize(ts.LogLikelihoodLR, ts.LogPriorLR, gp[i].Proposal, new int[] { 0, 1, 2 }, mother.state);
            //}

            //// single chain
            //int numSubModels = 1;
            //AuxiliaryChain[] submodel = new AuxiliaryChain[] { new AuxiliaryChain() };
            //SimpleSimultaneousGaussianProposal[] gp = new SimpleSimultaneousGaussianProposal[] { new SimpleSimultaneousGaussianProposal(new double[] { 0.01 }) };
            //ParallelHierarchicalSampler mother = new ParallelHierarchicalSampler(total, burnin, numSubModels, 3);
            //mother.Initialize(ts.LogLikelihoodLR, ts.LogPriorLR, pars);
            //mother.mcmc[0].Initialize(ts.LogLikelihoodLR, ts.LogPriorLR, gp[0].Proposal, new int[] { 0, 1, 2 }, mother.state);

            mother.Run();
        }


        private List<double> linearRegression(double[] x, double[] y)
        {
            int n = x.Length;
            double xx = 0;
            double yy = 0;
            double xy = 0;
            double meanx = 0;
            double meany = 0;
            for (int i = 0; i < n; i++)
            {
                meanx += x[i];
                meany += y[i];
            }
            meanx /= n;
            meany /= n;

            for (int i = 0; i < n; i++)
            {
                xx += Math.Pow(x[i] - meanx, 2);
                yy += Math.Pow(y[i] - meany, 2);
                xy += (x[i] - meanx) * (y[i] - meany);
            }

            double b = xy / xx;
            double a = meany - meanx * b;

            double tau = 0;
            for (int i = 0; i < n; i++)
            {
                tau += Math.Pow(y[i] - a - b * x[i], 2);
            }
            tau /= (n - 1);
            double logL = -0.5 * Math.Log(n * tau) - 0.5 * (n - 1);
            return new List<double> { a, b, tau, logL };
        }

        private void semipartitionButton_Click(object sender, RoutedEventArgs e)
        {
            TestSemipartition ts = new TestSemipartition();
            ts.Test();
        }

        private void clusterButton_Click(object sender, RoutedEventArgs e)
        {
            int nElements = 100;
            List<double> elements = new List<double>();
            Troschuetz.Random.Distributions.Continuous.ContinuousUniformDistribution cud = new Troschuetz.Random.Distributions.Continuous.ContinuousUniformDistribution(0, 1);
            for (int i= 0; i < nElements; i++)
            {
                elements.Add(cud.NextDouble());
            }

            Ball<double>.Metric metric = (double x, double y) => Math.Abs(x - y);
            Tuple<List<Ball<double>>, bool, int> tuple = Ball<double>.Cluster2FixedNumberOfBalls(elements, metric, 10, 10000);

            List<Ball<double>> balls = tuple.Item1;

            using (System.IO.StreamWriter writer = System.IO.File.CreateText("testClusterer.txt"))
            {
                foreach (Ball<double> ball in balls)
                {
                    foreach (double element in ball.Elements)
                    {
                        writer.WriteLine(element + "\t" + ball.Center + "\t" + ball.Radius + "\t" + Math.Abs(element - ball.Center));
                    }
                }
            }

            MessageBox.Text += "Cluster done.\n";
            MessageBox.Text += "Converged = " + tuple.Item2 + "\n";
            MessageBox.Text += "n Steps taken = " + tuple.Item3 + "\n";
        }

        private void networkButton_Click(object sender, RoutedEventArgs e)
        {
            Network<int> network = new Network<int>();
            network.AttachNode(1, new List<int>() { 2 });
            network.AttachNode(2, new List<int>() { 3 });
            network.AttachNode(4, new List<int>(){5});
            network.AttachNode(5, new List<int>() { 6 });

            List<Network<int>> connectedComponentsList = Network<int>.GetConnectedComponents(network);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            int size = smSizeBox. AsInt();
            double[] times = TestScoreManager.Test(size);
            MessageBox.Text += "Purge time = " + times[0] + "\n";
            MessageBox.Text += "Reval time = " + times[1] + "\n";
        }

        private void logStarClick(object sender, RoutedEventArgs e)
        {
            double y = yBox.AsDouble(1);
            double logstary = Utilities.LogStar(y);
            MessageBox.Text += "Log*(" + y + ") = " + logstary + "\n";
        }

        private void reroot_Click(object sender, RoutedEventArgs e)
        {
            //Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            //ofd.Title = "Open Newick file with tree.";
            //ofd.Filter = "Newick file|*.nwk|all files|*.*";
            //if (!(bool)ofd.ShowDialog())
            //    return;

            //string treeString = System.IO.File.ReadAllText(ofd.FileName);

            string treeString = "(c5970S1V11:0.03073,(MRCA:0.00006,c8136S1V8:0.00249):0.00250,c8756S1V5:0.02533)";
            Tree<string> tree = new Tree<string>(treeString);

            //            tree = TestFunctions.RerootTree(tree, rootNameBox.Text);
            tree = TestFunctions.RerootTree(tree, "MRCA");
            treeString = tree.GetDescriptor();

            System.IO.File.WriteAllText("rerooted.nwk", treeString);
            MessageBox.Text += "Done.\n";
        }

        private void Condense_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Title = "Open Newick file with tree.";
            ofd.Filter = "Newick file|*.nwk|all files|*.*";
            if (!(bool)ofd.ShowDialog())
                return;


            string treeString = System.IO.File.ReadAllText(ofd.FileName);
            Tree<string> tree = new Tree<string>(treeString);

            double theta = double.Parse(rootNameBox.Text);
            Tree<int> newTree = TestFunctions.CondenseTree(tree, theta);
            int first = 1;
            Tree<int>.NameIntermediates(newTree, ref first);
            concateNamesAndContents(newTree);
            treeString = newTree.GetDescriptor();

            System.IO.File.WriteAllText("condensed.nwk", treeString);
            MessageBox.Text += "Done.\n";
        }

        private void concateNamesAndContents(Tree<int> tree)
        {
            foreach (Tree<int> child in tree.Children)
            {
                concateNamesAndContents(child);
            }
            tree.Name += "|" + tree.Contents.ToString();
        }

        private void GetTreeAges_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Title = "Open Newick file with tree.";
            ofd.Filter = "Newick file|*.nwk|all files|*.*";
            if (!(bool)ofd.ShowDialog())
                return;

            string treeString = System.IO.File.ReadAllText(ofd.FileName);

            Dictionary<string, double> ages = new Dictionary<string, double>();
            Tree<string> tree = new Tree<string>(treeString);
            int next = 0;
            Tree<string>.NameIntermediates(tree, ref next);
            List<string> nodeNames = tree.getInternalNodeNames();
            

        }

    }
}
