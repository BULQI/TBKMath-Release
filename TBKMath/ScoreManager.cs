using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBKMath
{
    public class ScoreManager<T> : IEnumerable
    {
        private bool useNames = false;
        private List<double> scores;
        private List<T> items;
        private List<bool> activities;
        private List<string> names;
        public T TopItem;
        public double TopScore;
        public double Threshold = 4.605;

        public ScoreManager(bool useNames = false)
        {
            this.useNames = useNames;
            scores = new List<double>();
            items = new List<T>();
            activities = new List<bool>();
            names = new List<string>();
            TopScore = double.MinValue;
        }

        public bool Add(T item, double score)
        {
            if (score < TopScore - Threshold)
                return false;

            if (items.Contains(item))
                return false;

            items.Add(item);
            scores.Add(score);
            activities.Add(true);

            if (useNames)
            {
                names.Add(item.ToString());
            }

            if (score > TopScore)
            {
                TopScore = score;
                TopItem = item;
            }
            return true;
        }

        public bool Add(ScoredObject<T> so)
        {
            if (so.Score < TopScore - Threshold)
                return false;

            if (items.Contains(so.Item))
                return false;

            items.Add(so.Item);
            scores.Add(so.Score);
            activities.Add(true);

            if (useNames)
            {
                names.Add(so.Item.ToString());
            }

            if (so.Score > TopScore)
            {
                TopScore = so.Score;
                TopItem = so.Item;
            }
            return true;
        }

        public int Count
        {
            get { return activities.Count<bool>(b => b); }
        }

        public void ExponentiateAndNormalize()
        {
            double sum = 0;

            for (int i = 0; i < scores.Count; i++)
            {
                if (activities[i])
                {
                    double p = Math.Exp(scores[i] - TopScore);
                    scores[i] = p;
                    sum += p;
                }
            }

            if (sum == 0)
                return;

            for (int i = 0; i < scores.Count; i++)
            {
                if (activities[i])
                {
                    scores[i] /= sum;
                }
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
                if (activities[i])
                {
                    p = Math.Exp(scores[i] - TopScore);
                    sum += p;
                }
            }
            return Math.Log(sum) + TopScore;
        }

        public void Purge()
        {
            List<double> tmpScores = new List<double>(scores.Count);
            List<T> tmpItems = new List<T>(scores.Count);
            List<bool> tmpActivities = new List<bool>();
            List<string> tmpNames = new List<string>();

            double thresholdScore = TopScore - Threshold;

            for (int i = 0; i < scores.Count; i++)
            {
                if (scores[i] >= thresholdScore)
                {
                    tmpScores.Add(scores[i]);
                    tmpItems.Add(items[i]);
                    tmpActivities.Add(true);
                    if (useNames)
                        tmpNames.Add(items[i].ToString());
                } 
            }

            scores = tmpScores;
            items = tmpItems;
            activities = tmpActivities;
            names = tmpNames;
        }

        public bool Remove(T item)
        {
            int index = items.FindIndex(x => x.Equals(item));
            if (index > -1)
            {
                items.RemoveAt(index);
                activities.RemoveAt(index);
                if (scores[index]  == TopScore)
                {
                    scores.RemoveAt(index);
                    RecomputeTopScore();
                }
                else
                {
                    scores.RemoveAt(index);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Revalidate() 
        {
            double thresholdScore = TopScore - Threshold;
            for (int i = 0; i < scores.Count; i++)
            {
                activities[i] = scores[i] >= thresholdScore;
            }
        }

        public void Deactivate(T item)
        {
            int index = items.IndexOf(item);

            if (index > -1)
                activities[index] = false;
        }

        public void Activate(T item)
        {
            int index = items.IndexOf(item);

            if (index > -1)
                activities[index] = true;
        }

        public List<T> ItemsWithScoresGreaterThan(double minScore)
        {
            List<T> list = new List<T>();
            for (int i= 0; i< items.Count; i++)
            {
                if (scores[i] > minScore)
                {
                    list.Add(items[i]);
                }
            }
            return list;
        }

        public List<ScoredObject<T>> ScoredItemsWithScoresGreaterThan(double minScore)
        {
            List<ScoredObject<T>> list = new List<ScoredObject<T>>();
            for (int i = 0; i < items.Count; i++)
            {
                if (scores[i] > minScore)
                {
                    list.Add(new ScoredObject<T>() { Item = items[i], Score = scores[i] });
                }
            }
            return list;
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

        public bool ContainsItemNamed(string name)
        {
            return names.Contains(name);
        }

        public T ItemByName(string name)
        {
            int i = names.IndexOf(name);
            if (i > -1)
            {
                return items[i];
            }
            else
            {
                throw new ArgumentException("No item with that name was found.");
            }
        }

        public IEnumerator GetEnumerator()
        {
            return new ScoreManagerEnum<T>(items, activities, scores);
        }

    }
    public class ScoreManagerEnum<T> : IEnumerator
    {
        private List<T> members;
        private List<bool> activities;
        private List<double> scores;
        int position = -1;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="_members">Pass the members field of the PolynucCollection being enumerated.</param>
        public ScoreManagerEnum(List<T> _members, List<bool> _activities, List<double> _scores)
        {
            members = _members;
            activities = _activities;
            scores = _scores; 
        }

        /// <summary>
        /// Advances to the next item in the collection.
        /// </summary>
        /// <returns>The enumerator positioned at the next item.</returns>
        public bool MoveNext()
        {
            position++;
            while (position < activities.Count && !activities[position])
            {
                position++;
            }
            return (position < members.Count);
        }

        /// <summary>
        /// Empty method included to implement the interface.
        /// </summary>
        public void Reset()
        {
            position = -1;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        /// <summary>
        /// Gets the current item.
        /// </summary>
        public ScoredObject<T> Current
        {
            get
            {
                try
                {
                    return new ScoredObject<T>() { Item = members[position], Score = scores[position] };
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}
