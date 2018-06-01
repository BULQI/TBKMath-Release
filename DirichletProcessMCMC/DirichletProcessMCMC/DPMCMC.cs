using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirichletProcessMCMC
{
    public class DPMCMC<T>
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
        private Dictionary<HashSet<T>, double> priorProbability;
        private int totalNumber;
        private List<T> entities;
        private HashSet<HashSet<T>> partition;
        private Dictionary<T, HashSet<T>> assignments;

        private void ComputePriorProbabilities()
        {
            if (priorProbability == null)
            {
                priorProbability = new Dictionary<HashSet<T>, double>();
            }

            foreach (HashSet<T> block in partition)
            {
                if (!priorProbability.ContainsKey(block))
                {
                    priorProbability.Add(block, double.NaN);
                }
                priorProbability[block] = block.Count / (totalNumber + alpha);
            }
        }

        private bool RemoveEntity(T entity)
        {
            if (!assignments.ContainsKey(entity))
            {
                return false;
            }

            assignments[entity].Remove(entity);
            ComputePriorProbability(assignments[entity]);
            assignments[entity] = null;
            return true;
        }

        private bool AddEntity(T entity, HashSet<T> block)
        {
            if (block == null)
            {
                block = new HashSet<T>();
                block.Add(entity);
                if (!assignments.ContainsKey(entity))
                {
                    assignments.Add(entity, block);
                }
                else
                {
                    assignments[entity] = block;
                }
                ComputePriorProbability(block);
            }
            return true;
        }

        private void ComputePriorProbability(HashSet<T> block)
        {
            if (!priorProbability.ContainsKey(block))
            {
                priorProbability.Add(block, double.NaN);
            }
            priorProbability[block] = block.Count / (alpha + totalNumber);
        }

        private double Likelihood(HashSet<T> block)
        {
            // this will be replaced by a delegate
            return 0;
        }

        private double Likelihood(T entity, HashSet<T> block)
        {
            return 0;
        }
    }
}
