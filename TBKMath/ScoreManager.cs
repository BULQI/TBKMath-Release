using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBKMath
{
    public class ScoredObject<T> : IComparable<ScoredObject<T>>
    {
        public T Item;
        public double Score;
        public bool Active;

        public int CompareTo(ScoredObject<T> other)
        {
            if (other == null)
                return 1;

            if (Score < other.Score)
            {
                return -1;
            }
            else if (Score > other.Score)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public static bool operator > (ScoredObject<T> so1, ScoredObject<T> so2)
        {
            return so1.CompareTo(so2) == 1;
        }

        public static bool operator < (ScoredObject<T> so1, ScoredObject<T> so2)
        {
            return so1.CompareTo(so2) == -1;
        }

        public static bool operator >= (ScoredObject<T> so1, ScoredObject<T> so2)
        {
            return so1.CompareTo(so2) >= 0;
        }

        public static bool operator <= (ScoredObject<T> so1, ScoredObject<T> so2)
        {
            return so1.CompareTo(so2) <= 0;
        }
    }

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

        public void Sort()
        {

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

    public class ScoreManager2<T> : IEnumerable
    {
        private bool useNames = false;
        private List<ScoredObject<T>> scoredObjects;
        private List<string> names;
        public T TopItem;
        public double TopScore;
        public double Threshold = 4.605;
        private bool isNormalized = false;
        public bool IsNormalized
        {
            get { return isNormalized; }
        }

        public ScoreManager2(bool useNames = false)
        {
            this.useNames = useNames;
            scoredObjects = new List<ScoredObject<T>>();
            names = new List<string>();
            TopScore = double.MinValue;
        }

        public bool Add(T item, double score)
        {
            if (score < TopScore - Threshold)
                return false;

            if (useNames)
            {
                if (names.Contains(item.ToString()))
                    return false;

                names.Add(item.ToString());
            }

            scoredObjects.Add(new ScoredObject<T>() { Active = true, Item = item, Score = score });
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

            if (useNames)
            {
                if (names.Contains(so.Item.ToString()))
                    return false;

                names.Add(so.Item.ToString());
            }

            scoredObjects.Add(so);
            if (so.Score > TopScore)
            {
                TopScore = so.Score;
                TopItem = so.Item;
            }

            if (so.Score >= TopScore - Threshold)
            {
                so.Active = true;
            }
            else
            {
                so.Active = false;
            }
            return true;
        }

        public int Count
        {
            get { return scoredObjects.Count(so => so.Active == true); }
        }

        public void ExponentiateAndNormalize()
        {
            double sum = 0;
            for (int i= 0; i< scoredObjects.Count; i++)
            {
                if (scoredObjects[i].Active)
                {
                    double p = Math.Exp(scoredObjects[i].Score - TopScore);
                    scoredObjects[i].Score = p;
                    sum += p;
                }
            }

            if (sum == 0)
                return;

            for (int i = 0; i < scoredObjects.Count; i++)
            {
                scoredObjects[i].Score /= sum;
            }
            TopScore = 1.0 / sum;
            isNormalized = true;
        }

        public void Sort()
        {
            scoredObjects.Sort(new Comparison<ScoredObject<T>>((so1, so2) => so2.CompareTo(so1)));
        }

        public double GetBayesFactor()
        {
            if (scoredObjects.Count == 0)
                return double.NegativeInfinity;

            double sum = 0;
            double p;
            for (int i = 0; i < scoredObjects.Count; i++)
            {
                if (scoredObjects[i].Active)
                {
                    p = Math.Exp(scoredObjects[i].Score - TopScore);
                    sum += p;
                }
            }
            return Math.Log(sum) + TopScore;
        }

        public void Purge()
        {
            List<ScoredObject<T>> tmpSOs = new List<ScoredObject<T>>();
            List<string> tmpNames = new List<string>();

            double thresholdScore = TopScore - Threshold;

            for (int i = 0; i < scoredObjects.Count; i++)
            {
                if (scoredObjects[i].Score >= thresholdScore)
                {
                    tmpSOs.Add(scoredObjects[i]);
                    if (useNames)
                        tmpNames.Add(scoredObjects[i].Item.ToString());
                }
            }

            scoredObjects = tmpSOs;
            names = tmpNames;
        }

        public bool Remove(T item)
        {
            int index = names.FindIndex(name => name==item.ToString());
            if (index > -1)
            {
                names.RemoveAt(index);
                scoredObjects.RemoveAt(index);
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
            for (int i = 0; i < scoredObjects.Count; i++)
            {
                scoredObjects[i].Active = scoredObjects[i].Score >= thresholdScore;
            }
        }

        public void RecomputeTopScore()
        {
            TopScore = double.MinValue;
            for (int i = 0; i < scoredObjects.Count; i++)
            {
                if (scoredObjects[i].Score > TopScore)
                {
                    TopScore = scoredObjects[i].Score;
                    TopItem = scoredObjects[i].Item;
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
                return scoredObjects[i].Item;
            }
            else
            {
                throw new ArgumentException("No item with that name was found.");
            }
        }

        public IEnumerator GetEnumerator()
        {
            return new ScoreManager2Enum<T>(scoredObjects);
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

    public class ScoreManager2Enum<T> : IEnumerator
    {
        private List<ScoredObject<T>> scoredObjects;
        private List<string> names;
        int position = -1;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="_members">Pass the members field of the PolynucCollection being enumerated.</param>
        public ScoreManager2Enum(List<ScoredObject<T>> scoredObjects)
        {
            this.scoredObjects = scoredObjects;
        }

        /// <summary>
        /// Advances to the next item in the collection.
        /// </summary>
        /// <returns>The enumerator positioned at the next item.</returns>
        public bool MoveNext()
        {
            position++;
            while (position < scoredObjects.Count && !scoredObjects[position].Active)
            {
                position++;
            }
            return (position < scoredObjects.Count);
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
                    return scoredObjects[position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }

}
