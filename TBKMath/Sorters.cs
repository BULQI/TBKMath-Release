﻿using System;
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
            WorstScore = double.MinValue;
        }

        public string Serialized()
        {
            string output = string.Empty;

            DictionarySorter<T>.SortDictionary(Scores);

            foreach (T t in Scores.Keys)
            {
                output += t.ToString() + "\t" + Scores[t].ToString() + "\n";
            }
            return output;
        }

        public bool Process(T item, double score)
        {
            // just a check...
            if (score < WorstScore && Scores.Count >= NKeep)
            {
                return false;
            }

            else if (Scores.ContainsKey(item))
            {
                throw new Exception("Item already exists in scores.");
            }
            else
            {
                Scores.Add(item, score);
                if (Scores.Count > NKeep)
                {
                    WorstScore = double.MaxValue;
                    foreach (KeyValuePair<T, double> kvp in Scores)
                    {
                        if (kvp.Value <= WorstScore)
                        {
                            WorstScore = kvp.Value;
                            worstResult = kvp.Key;
                        }
                    }
                    Scores.Remove(worstResult);
                }
                DictionarySorter<T>.SortDictionary(Scores);
                return true;
            }

            // find new worst result
            //WorstScore = double.MaxValue;
            //foreach (KeyValuePair<T, double> kvp in Scores)
            //{
            //    if (kvp.Value <= WorstScore)
            //    {
            //        WorstScore = kvp.Value;
            //        worstResult = kvp.Key;
            //    }
            //   
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

        public ScoreKeeper(ScoreManager<T> manager)
        {
            
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
