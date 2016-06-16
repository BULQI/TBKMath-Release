using System;
using System.Collections.Generic;

// Demonstrate a Priority Queue implemented with a Binary Heap
// 
//    By James McCaffrey
//    11/02/2012
//
// http://visualstudiomagazine.com/articles/2012/11/01/priority-queues-with-c.aspx


namespace TBKMath
{
    public struct ScoredObject<T>
    {
        public T Item;
        public double Score;
    }

    //public class PriorityQueue<T> where T : IComparable<T>
    //{
    //    private List<T> data;
    //    public PriorityQueue()
    //    {
    //        this.data = new List<T>();
    //    }

    //    public void Enqueue(T item)
    //    {
    //        data.Add(item);
    //        int ci = data.Count - 1; // child index; start at end
    //        while (ci > 0)
    //        {
    //            int pi = (ci - 1) / 2; // parent index
    //            if (data[ci].CompareTo(data[pi]) >= 0) // child item is larger than (or equal) parent so we're done
    //                break;
    //            T tmp = data[ci];
    //            data[ci] = data[pi];
    //            data[pi] = tmp;
    //            ci = pi;
    //        }
    //    }

    //    public T Dequeue()
    //    {
    //        // assumes pq is not empty; up to calling code
    //        int li = data.Count - 1; // last index (before removal)
    //        T frontItem = data[0];   // fetch the front
    //        data[0] = data[li];
    //        data.RemoveAt(li);

    //        --li; // last index (after removal)
    //        int pi = 0; // parent index. start at front of pq
    //        while (true)
    //        {
    //            int ci = pi * 2 + 1; // left child index of parent
    //            if (ci > li) break;  // no children so done
    //            int rc = ci + 1;     // right child
    //            if (rc <= li && data[rc].CompareTo(data[ci]) < 0) // if there is a rc (ci + 1), and it is smaller than left child, use the rc instead
    //                ci = rc;
    //            if (data[pi].CompareTo(data[ci]) <= 0) break; // parent is smaller than (or equal to) smallest child so done
    //            T tmp = data[pi]; data[pi] = data[ci]; data[ci] = tmp; // swap parent and child
    //            pi = ci;
    //        }
    //        return frontItem;
    //    }

    //    public T Peek()
    //    {
    //        T frontItem = data[0];
    //        return frontItem;
    //    }

    //    public int Count()
    //    {
    //        return data.Count;
    //    }

    //    public override string ToString()
    //    {
    //        string s = "";
    //        for (int i = 0; i < data.Count; ++i)
    //            s += data[i].ToString() + " ";
    //        s += "count = " + data.Count;
    //        return s;
    //    }

    //    public bool IsConsistent()
    //    {
    //        // is the heap property true for all data?
    //        if (data.Count == 0) return true;
    //        int li = data.Count - 1; // last index
    //        for (int pi = 0; pi < data.Count; ++pi) // each parent index
    //        {
    //            int lci = 2 * pi + 1; // left child index
    //            int rci = 2 * pi + 2; // right child index

    //            if (lci <= li && data[pi].CompareTo(data[lci]) > 0) return false; // if lc exists and it's greater than parent then bad.
    //            if (rci <= li && data[pi].CompareTo(data[rci]) > 0) return false; // check the right child too.
    //        }
    //        return true; // passed all checks
    //    } // IsConsistent
    //} // PriorityQueue

    public class PriorityQueue<T>
    {
        private List<T> items;
        private List<double> scores;
        public double MaxScore
        {
            get 
            {
                if (scores.Count > 0)
                    return scores[0];
                else
                    return double.MinValue;
            }
        }

        public double MinScore
        {
            get { return scores[scores.Count - 1]; }
        }

        public Tuple<T, double> this[int i]
        {
            get { return new Tuple<T, double>(item1: items[i], item2: scores[i]); }
        }

        public void ExponentiateAndNormalize()
        {
            double sum = 0;

            double maxScore = scores[0];
            for (int i = 0; i < scores.Count; i++)
            {
                double p = Math.Exp(scores[i] - maxScore);
                scores[i] = p;
                sum += p;
            }

            if (sum == 0)
                return;

            for (int i = 0; i < scores.Count; i++)
            {
                scores[i] /= sum;
            }
        }

        public PriorityQueue()
        {
            this.scores = new List<double>();
            this.items = new List<T>();
        }

        public void Enqueue(T item, double score)
        {
            items.Add(item);
            scores.Add(score);
            int ci = scores.Count - 1; // child index; start at end
            while (ci > 0)
            {
                int pi = (ci - 1) / 2; // parent index
                if (scores[ci] <= scores[pi]) // child item is smaller than (or equal to) parent so we're done
                    break;
                double tmpScore = scores[ci];
                T tmpItem = items[ci];

                scores[ci] = scores[pi];
                items[ci] = items[pi];
                
                scores[pi] = tmpScore;
                items[pi] = tmpItem;

                ci = pi;
            }
        }

        public ScoredObject<T> Dequeue()
        {
            // assumes pq is not empty; up to calling code
            int li = scores.Count - 1; // last index (before removal)
            ScoredObject<T> frontItem = new ScoredObject<T>(){ Item = items[0], Score = scores[0]};   // fetch the front
            scores[0] = scores[li];
            scores.RemoveAt(li);
            items[0] = items[li];
            items.RemoveAt(li);

            --li; // last index (after removal)
            int pi = 0; // parent index. start at front of pq
            while (true)
            {
                int ci = pi * 2 + 1; // left child index of parent
                if (ci > li) break;  // no children so done
                int rc = ci + 1;     // right child
                if (rc <= li && scores[rc] > scores[ci]) // if there is a rc (ci + 1), and it is smaller than left child, use the rc instead
                    ci = rc;
                if (scores[pi] >= scores[ci]) break; // parent is smaller than (or equal to) smallest child so done
                double tmpScore = scores[pi];
                T tmpItem = items[pi];
                scores[pi] = scores[ci];
                items[pi] = items[ci];
                scores[ci] = tmpScore; // swap parent and child
                items[ci] = tmpItem;
                pi = ci;
            }
            return frontItem;
        }

        public ScoredObject<T> Peek()
        {
            ScoredObject<T> frontItem = new ScoredObject<T>() { Score = scores[0], Item = items[0] };
            return frontItem;
        }

        public int Count()
        {
            return scores.Count;
        }

        public bool IsConsistent()
        {
            // is the heap property true for all data?
            if (scores.Count == 0) return true;
            int li = scores.Count - 1; // last index
            for (int pi = 0; pi < scores.Count; ++pi) // each parent index
            {
                int lci = 2 * pi + 1; // left child index
                int rci = 2 * pi + 2; // right child index

                if (lci <= li && scores[pi] < scores[lci]) return false; // if lc exists and it's greater than parent then bad.
                if (rci <= li && scores[pi] < scores[rci]) return false; // check the right child too.
            }
            return true; // passed all checks
        } // IsConsistent
    } // PriorityQueue

}
