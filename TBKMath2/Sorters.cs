using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBKMath
{
    public class RankManager<T>
    {
        public int NKeep;
        public Dictionary<T, double> Scores;
        public double WorstScore;
        private T worstResult;

        public RankManager(int nKeep)
        {
            NKeep = nKeep;
            Scores = new Dictionary<T, double>();
        }

        public string Serialized()
        {
            string output = string.Empty;
            // sort 

            DictionarySorter<T>.SortDictionary(Scores);

            foreach (T t in Scores.Keys)
            {
                output += t.ToString() + "\t" + Scores[t].ToString();
            }
            return output;
        }

        public bool Process(T item, double score)
        {
            if (score < WorstScore)
                return false;

            if (Scores.ContainsKey(item))
            {
                throw new Exception("Item already exists in scores.");
            }

            Scores.Add(item, score);
            if (Scores.Count > NKeep)
            {
                Scores.Remove(worstResult);
            }

            // find new worst result
            WorstScore = double.MaxValue;
            foreach (KeyValuePair<T, double> kvp in Scores)
            {
                if (kvp.Value <= WorstScore)
                {
                    WorstScore = kvp.Value;
                    worstResult = kvp.Key;
                }
            }

            return true;
        }
    }

    public class ScoreManager<T>
    {
        public double MaxAllowableDifference;
        public Dictionary<T, double> Scores;
        public double MaxScore;
        private T argMax;
        public T ArgMax
        {
            get { return argMax; }
        }

        public ScoreManager(double maxDifference, double initialScore = double.MinValue)
        {
            MaxAllowableDifference = maxDifference;
            MaxScore = initialScore;
            Scores = new Dictionary<T, double>();
        }

        public string Serialized()
        {
            string output = string.Empty;
            // sort 

            DictionarySorter<T>.SortDictionary(Scores);

            foreach (T t in Scores.Keys)
            {
                output += t.ToString() + "\t" + Scores[t].ToString();
            }
            return output;
        }

        public bool Process(T item, double score)
        {
            if (score < MaxScore - MaxAllowableDifference)
                return false;

            if (Scores.ContainsKey(item))
            {
                throw new Exception("Item already exists in scores.");
            }

            if (score > MaxScore)
            {
                MaxScore = score;
                argMax = item;
            }

            Scores.Add(item, score);
            return true;
        }

        public void Purge()
        {
            foreach (T entity in Scores.Keys.ToArray())
            {
                if (Scores[entity] < MaxScore - MaxAllowableDifference)
                {
                    Scores.Remove(entity);
                }
            }
        }

        public bool Remove(T item)
        {
            if (!Scores.ContainsKey(item))
            {
                return false;
            }
            bool needNewMax = Scores[item] == MaxScore;
            Scores.Remove(item);
            if (needNewMax)
            {
                MaxScore = double.MinValue;
                foreach (KeyValuePair<T, double> kvp in Scores)
                {
                    if (kvp.Value > MaxScore)
                    {
                        MaxScore = kvp.Value;
                        argMax = kvp.Key;
                    }
                }
            }
            return true;
        }
    }

    /// <summary>
    /// Provides a the means to sort dictionaries on their values.
    /// </summary>
    /// <typeparam name="T">The data type of the dictionary key.</typeparam>
    public static class DictionarySorter<T>
    {
        /// <summary>
        /// Performs a sort of a dictionary (in place) by value.
        /// </summary>
        /// <param name="dic">The dictionary to be sorted.</param>
        public static void SortDictionary(Dictionary<T, double> dic)
        {
            List<KeyValuePair<T, double>> tempList = dic.ToList();
            tempList.Sort(
                (firstPair, nextPair) =>
                {
                    return nextPair.Value.CompareTo(firstPair.Value);
                }
            );
            dic.Clear();
            foreach (KeyValuePair<T, double> kvp in tempList)
            {
                dic.Add(kvp.Key, kvp.Value);
            }
        }
    }

    public class ScoreKeeper<T>
    {
        private List<double> scores;
        private List<T> items;
        public T TopItem;
        public double TopScore;

        public ScoreKeeper()
        {
            scores = new List<double>();
            items = new List<T>();
            TopScore = double.MinValue;
        }

        public void Add(T item, double score)
        {
            items.Add(item);
            scores.Add(score);
            if (score > TopScore)
            {
                TopScore = score;
                TopItem = item;
            }
        }

        public void Add(ScoredObject<T> so)
        {
            items.Add(so.Item);
            scores.Add(so.Score);
            if (so.Score > TopScore)
            {
                TopScore = so.Score;
                TopItem = so.Item;
            }
        }

        public ScoredObject<T> this[int i]
        {
            get { return new ScoredObject<T>() { Item = items[i], Score = scores[i] }; }
        }

        public int Count
        {
            get { return scores.Count; }
        }

        public void ExponentiateAndNormalize()
        {
            double sum = 0;

            for (int i = 0; i < scores.Count; i++)
            {
                double p = Math.Exp(scores[i] - TopScore);
                scores[i] = p;
                sum += p;
            }

            if (sum == 0)
                return;

            for (int i = 0; i < scores.Count; i++)
            {
                scores[i] /= sum;
            }

            TopScore = 1.0 / sum;
        }

        public double GetBayesFactor()
        {
            if (scores.Count == 0)
                return double.NegativeInfinity;

            double sum = 0;
            double p;
            for (int i = 0; i < scores.Count; i++)
            {
                p = Math.Exp(scores[i] - TopScore);
                sum += p;
            }
            return Math.Log(sum) + TopScore;
        }

        public void Purge(double threshold)
        {
            List<double> tmpScores = new List<double>(scores.Count);
            List<T> tmpItems = new List<T>(scores.Count);
            double thresholdScore = TopScore - threshold;

            for (int i = 0; i < scores.Count; i++ )
            {
                if (scores[i] >= thresholdScore)
                {
                    tmpScores.Add(scores[i]);
                    tmpItems.Add(items[i]); 
                }
            }
            scores = tmpScores;
            items = tmpItems;

            //for (int i = scores.Count - 1; i >= 0; i--)
            //{
            //    if (scores[i] < TopScore - threshold)
            //    {
            //        scores.RemoveAt(i);
            //        items.RemoveAt(i);
            //    }
            //}
        }

        public void RemoveAt(int i)
        {
            double score = scores[i];
            scores.RemoveAt(i);
            items.RemoveAt(i);
            if (score == TopScore)
                RecomputeTopScore();
        }

        public void RecomputeTopScore()
        {
            TopScore = double.MinValue;
            for (int i = 0; i < scores.Count; i++)
            {
                if (scores[i] > TopScore)
                {
                    TopScore = scores[i];
                    TopItem = items[i];
                }
            }
        }

    }



}
