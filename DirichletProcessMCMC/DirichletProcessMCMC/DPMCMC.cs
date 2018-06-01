using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBKMath;

namespace DirichletProcessMCMC
{
    public class DPMCMC<T> where T: new()
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
        private HashSet<HashSet<T>> partition;
        private Dictionary<HashSet<T>, double> priorProbability;
        private Dictionary<HashSet<T>, double> likelihood;
        private Dictionary<T, HashSet<T>> assignments;
        private GeneralDiscreteDistribution<HashSet<T>> probBlock;
        private GeneralDiscreteDistribution<T> probObject;

        public DPMCMC(T[] _entities, double _alpha)
        {
            entities = _entities;
            totalNumber = _entities.Length;
            alpha = _alpha;

            double[] p = new double[totalNumber];
            double q = 1 / totalNumber;
            for (int i = 0; i < totalNumber; i++) { p[i] = q; }
            probObject = new GeneralDiscreteDistribution<T>(entities, p);
            partition = new HashSet<HashSet<T>>();
            partition.Add(new HashSet<T>());
            assignments = new Dictionary<T, HashSet<T>>();
        }

        public void Initialize()
        {
            workingNumber = 0;
            foreach (T entity in entities)
            {
                double[] p = new double[partition.Count];
                int i = 0;
                foreach (HashSet<T> block in partition)
                {
                    p[i] = computePosteriorLikelihood(block, entity);
                    i++;
                }
                double sum = p.Sum();
                for (i =0; i < p.Length; i++)
                {
                    p[i] /= sum;
                }
                probBlock = new GeneralDiscreteDistribution<HashSet<T>>(partition.ToArray(), p);
                HashSet<T> chosenblock = probBlock.Next();
                AddEntity(entity, chosenblock);
                workingNumber++;
            }
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
            return p * Likelihood(entity, block);
        }

        private bool RemoveEntity(T entity)
        {
            if (!assignments.ContainsKey(entity))
            {
                return false;
            }

            assignments[entity].Remove(entity);
            if (assignments[entity].Count == 0)
            {
                partition.Remove(assignments[entity]);
            }
            else
            {
                assignments[entity] = null;
            }
            return true;
        }

        private bool AddEntity(T entity, HashSet<T> block)
        {
            if (block.Count == 0)
            {
                partition.Add(new HashSet<T>());
            }

            block.Add(entity);
            if (!assignments.ContainsKey(entity))
            {
                assignments.Add(entity, block);
            }
            else
            {
                assignments[entity] = block;
                partition.Add(block);
            }
            return true;
        }

        //private void ComputePriorProbability(HashSet<T> block)
        //{
        //    if (!priorProbability.ContainsKey(block))
        //    {
        //        priorProbability.Add(block, double.NaN);
        //    }
        //    priorProbability[block] = block.Count / (alpha + totalNumber);
        //}

        private double Likelihood(HashSet<T> block)
        {
            // this will be replaced by a delegate
            return 1;
        }

        private double Likelihood(T entity, HashSet<T> block)
        {
            return 1;
        }
    }
}
