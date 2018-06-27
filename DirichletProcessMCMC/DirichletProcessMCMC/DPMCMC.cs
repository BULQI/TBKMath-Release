using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBKMath;

namespace DirichletProcessMCMC
{
    public delegate double LogLikelihoodDelegate<T>(HashSet<T> block);
    public class DPMCMC<T> where T : new()
    {
        // Start with a set of objects
        // The goal is to assign them to subsets when the number of subsets is unknown.
        // Priors on the existing subsets are determined by their occupation numbers and a parameter.
        // A likelihood function on subsets is provided by the user.
        // 
        // The algorithm proceeds stepwise.
        // On each step, an object is chosen at random and removed from its current subset, c.
        // The prior on c is adjusted.
        // The marginal probability of assignment to each cluster, including the null cluster, is computed
        // and the assignment is then made randomly according to the marginals.

        // the main data structure should be a set of subsets
        // must be able to select an entity at random
        // must be able to locate the subset to which the entity is assigned


        private double alpha;
        private int totalNumber;
        private int workingNumber;
        private T[] entities;
        private Dictionary<int, HashSet<T>> blocks;
        private Dictionary<int, double> logLikelihoods;
        private Dictionary<int, int> assignments;
        private GeneralDiscreteDistribution<int> probBlock;
        private GeneralDiscreteDistribution<int> probObject;
        private int samplingInterval = 100;
        private List<int> BlockIndices;
        private LogLikelihoodDelegate<T> logLikDel;
        private double sumLogLik;

        public DPMCMC(T[] _entities, double _alpha, LogLikelihoodDelegate<T> logLikelihoodDelegate)
        {
            entities = _entities;
            totalNumber = _entities.Length;
            alpha = _alpha;
            logLikDel = logLikelihoodDelegate;
            double[] p = new double[totalNumber];
            double q = 1.0 / totalNumber;
            for (int i = 0; i < totalNumber; i++) { p[i] = q; }

            int[] indices = new int[totalNumber];
            for (int i = 0; i < totalNumber; i++) { indices[i] = i; }
            probObject = new GeneralDiscreteDistribution<int>(indices, p);

            BlockIndices = new List<int>();
            for (int i = 0; i < totalNumber + 1; i++) { BlockIndices.Add(i); }

            blocks = new Dictionary<int, HashSet<T>>();
            blocks.Add(0, new HashSet<T>());
            logLikelihoods = new Dictionary<int, double>();
            logLikelihoods.Add(0, 0);
            BlockIndices.Remove(0);
            assignments = new Dictionary<int, int>();
        }

        public void Initialize()
        {
            workingNumber = 0;
            sumLogLik = 0;
            for (int i = 0; i < totalNumber; i++)
            {
                double[] p = new double[blocks.Count];
                int j = 0;
                foreach (KeyValuePair<int,HashSet<T>> kvp in blocks)
                {
                    p[j] = computePosteriorLikelihood(kvp.Value, entities[i]);
                    j++;
                }
                double sum = p.Sum();
                for (int k = 0; k < p.Length; k++)
                {
                    p[k] /= sum;
                }
                probBlock = new GeneralDiscreteDistribution<int>(blocks.Keys.ToArray(), p);
                int chosenblockIndex = probBlock.Next();
                AddEntity(i, chosenblockIndex);                
                workingNumber++;
            }
        }

        public string Run(int nSteps)
        {
            StringBuilder history = new StringBuilder();
            for (int i = 0; i < nSteps; i++)
            {
                Dictionary<int, int> occupationNumbers = new Dictionary<int, int>();
                foreach (KeyValuePair<int,HashSet<T>> block in blocks) { occupationNumbers.Add(block.Key, block.Value.Count); }
                int entityIndex = probObject.Next();
                RemoveEntity(entityIndex);
                workingNumber = entities.Length - 1;
                double[] p = new double[blocks.Count];
                int j = 0;
                foreach (KeyValuePair<int, HashSet<T>> kvp in blocks)
                {
                    p[j] = computePosteriorLikelihood(kvp.Value, entities[entityIndex]);
                    j++;
                }
                double sum = p.Sum();
                for (int k = 0; k < p.Length; k++)
                {
                    p[k] /= sum;
                }
                probBlock = new GeneralDiscreteDistribution<int>(blocks.Keys.ToArray(), p);
                int chosenblockIndex = probBlock .Next();
                AddEntity(entityIndex, chosenblockIndex);
                if (i % samplingInterval == 0)
                {
                    history.AppendLine(i + "\t" + (blocks.Count - 1) + "\t" + sumLogLik);
                }
            }
            return history.ToString();
        }

        private double computePosteriorLikelihood(HashSet<T> block, T entity)
        {
            double p = double.NaN;
            if (block.Count == 0)
            {
                p = alpha / (alpha + workingNumber);
            }
            else
            {
                p = block.Count / (alpha + workingNumber);
            }
            return p * Math.Exp(LogLikelihood(entity, block));
        }

        private bool RemoveEntity(int iEntity)
        {
            if (!assignments.ContainsKey(iEntity))
            {
                return false;
            }

            int iSource = assignments[iEntity];
            // keep track of the sum log likelihood
            sumLogLik -= logLikelihoods[iSource];

            blocks[iSource].Remove(entities[iEntity]);

            // if the source block is now empty, remove it
            // and recycle its index
            if (blocks[iSource].Count == 0)
            {
                // recycle block keys
                BlockIndices.Add(iSource);
                blocks.Remove(iSource);
                logLikelihoods.Remove(iSource);
            }
            else
            {
                // entity is formally unassigned
                assignments[iEntity] = -1;
                // compute the updated log likelihood
                logLikelihoods[iSource] = logLikDel(blocks[iSource]);
                // update the sum
                sumLogLik += logLikelihoods[iSource];
            }

            return true;
        }

        private bool AddEntity(int entityIndex, int blockIndex)
        {
            if (!blocks.ContainsKey(blockIndex))
            {
                throw new KeyNotFoundException();
            }

            // if adding to the null block, it is necessary to add a new empty block
            if (blocks[blockIndex].Count == 0)
            {
                // use the next available black index
                blocks.Add(BlockIndices[0],new HashSet<T>());
                logLikelihoods.Add(BlockIndices[0], 0);                
                BlockIndices.Remove(BlockIndices[0]);
            }

            // keep track of the sum logLikelihood
            sumLogLik -= logLikelihoods[blockIndex];
            blocks[blockIndex].Add(entities[entityIndex]);
            logLikelihoods[blockIndex] = logLikDel(blocks[blockIndex]);
            sumLogLik += logLikelihoods[blockIndex];
            
            // update the assignments
            if (!assignments.ContainsKey(entityIndex))
            {
                assignments.Add(entityIndex, blockIndex);
            }
            else
            {
                assignments[entityIndex] = blockIndex;
            }
            return true;
        }

        private double LogLikelihood(T entity, HashSet<T> block)
        {
            HashSet<T> temp = new HashSet<T>(block);
            temp.Add(entity);
            return logLikDel(temp);
        }
    }
}
