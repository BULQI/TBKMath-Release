using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TBKMath
{
    public class Block<T> where T : IEquatable<T>
    {
        private HashSet<T> members;
        public HashSet<T> Members
        {
            get { return members; }
        }

        private T connector;
        public T Connector
        {
            get { return connector; }
        }

        private Dictionary<T, double> scores;

        public Block()
        {
            members = new HashSet<T>();
            scores = new Dictionary<T, double>();
        }

        public Block(T connector)
        {
            this.connector = connector;
            members = new HashSet<T>();
            members.Add(connector);
            scores = new Dictionary<T, double>();
            scores.Add(connector, double.MaxValue);
        }

        public Block(T connector, IEnumerable<T> members)
        {
            this.connector = connector;
            this.members = new HashSet<T>(members);
            this.members.Add(connector);
            scores = new Dictionary<T, double>();
            scores.Add(connector, double.MaxValue);
        }

        public IEnumerator GetEnumerator()
        {
            return new BlockEnumST<T>(members);
        }

        public int Count
        {
            get { return members.Count; }
        }

        public override int GetHashCode()
        {
            if (connector == null)
            {
                return members.GetHashCode();
            }
            else
            {
                return members.GetHashCode() ^ connector.GetHashCode();
            }
        }

        public void AddMember(T item, double score = double.NaN)
        {
            if (!members.Contains(item))
            {
                members.Add(item);
                scores.Add(item, score);
            }
        }

        public void RemoveConnector()
        {
            if (members.Contains(Connector))
            {
                members.Remove(Connector);
            }
        }

        public void RemoveMember(T element)
        {
            members.Remove(element);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            return Equals((Block<T>)obj);
        }

        public bool Equals(Block<T> block)
        {
            if (connector == null)
            {
                bool val = members.SetEquals(block.members);
                return val;
            }
            else
            {
                bool val = members.SetEquals(block.members) && connector.Equals(block.connector);
                return val;
            }
        }

        public bool Contains(T item)
        {
            return members.Contains(item);
        }
    }

    /// <summary>
    /// Provides an enumerator so that the "foreach" construction can be used with Block
    /// </summary>
    public class BlockEnumST<T> : IEnumerator
    {
        private HashSet<T>.Enumerator enumerator;
        private HashSet<T> members;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="_members">Pass the members field of the PolynucCollection being enumerated.</param>
        public BlockEnumST(HashSet<T> _members)
        {
            members = _members;
            enumerator = members.GetEnumerator();
        }

        /// <summary>
        /// Advances to the next item in the collection.
        /// </summary>
        /// <returns>The enumerator positioned at the next item.</returns>
        public bool MoveNext()
        {
            return enumerator.MoveNext();
        }

        /// <summary>
        /// Empty method included to implement the interface.
        /// </summary>
        public void Reset() { }

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
        public T Current
        {
            get
            {
                try
                {
                    return enumerator.Current;
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }

    public class SearchTreeSymmetric<T> where T : IEquatable<T>
    {
        public HashSet<Block<T>> Blocks;
        public Tree<Block<T>> Tree;
        public Dictionary<HashSet<T>, double> Scores;
        public int MaxBlockSize;

        public SearchTreeSymmetric(IEnumerable<T> set, Dictionary<HashSet<T>, double> scores)
        {
            T root = FindCenter(set, scores);
            Block<T> block = new Block<T>(root, set);
            Blocks = new HashSet<Block<T>>() { block };
            Scores = scores;
            Tree = new Tree<Block<T>>(root.ToString());
            Tree.Contents = block;
        }

        public int ComputeMaxBlockSize()
        {
            MaxBlockSize = 0;
            ComputeMaxBlockSize(Tree);
            return MaxBlockSize;
        }

        public void ComputeMaxBlockSize(Tree<Block<T>> tree)
        {
            if (tree.Children.Count == 0)
            {
                if (tree.Contents.Count > MaxBlockSize)
                {
                    MaxBlockSize = tree.Contents.Count;
                }
            }
            foreach (Tree<Block<T>> child in tree.Children)
            {
                ComputeMaxBlockSize(child);
            }
        }

        public void Refine()
        {
            Refine(Tree);
        }

        public void Refine(Tree<Block<T>> tree)
        {
            if (tree.Children.Count == 0)
            {
                if (tree.Contents.Count > 1)
                {
                    HashSet<Block<T>> h = SplitBlock(tree.Contents, Scores);
                    foreach (Block<T> block in h)
                    {
                        Tree<Block<T>> t = new Tree<Block<T>>(block.Connector.ToString());
                        t.BranchLength = 1;
                        t.Contents = block;
                        tree.AddChild(t);
                    }
                }
            }
            else
            {
                foreach (Tree<Block<T>> child in tree.Children)
                {
                    Refine(child);
                }
            }
        }

        public static T FindCenter(IEnumerable<T> set, Dictionary<HashSet<T>, double> scores)
        {
            // maximize the minimum score over all members of the set
            Dictionary<T, double> minScores = new Dictionary<T, double>();
            foreach (T e0 in set)
            {
                double minScore = double.MaxValue;
                foreach (T e1 in set)
                {
                    if (e0.Equals(e1))
                        continue;

                    HashSet<T> hs = new HashSet<T>() { e0, e1 };
                    if (scores[hs] < minScore)
                    {
                        minScore = scores[hs];
                    }
                }
                minScores.Add(e0, minScore);
            }

            double maxMinScore = double.MinValue;
            T center = default(T);
            foreach (T e in minScores.Keys)
            {
                if (minScores[e] > maxMinScore)
                {
                    maxMinScore = minScores[e];
                    center = e;
                }
            }
            return center;
        }

        private static HashSet<Block<T>> SplitBlock(Block<T> b, Dictionary<HashSet<T>, double> scores)
        {
            /* See Cloanalyst Techinical Details, section SearchTree/SplitBlock.
            Briefly, splits b into a set of blocks such that the connectors in each block are as close as possible to b.Connector and 
            each the mebers of each block are connectable to b.Connector through its own block's connector.
            */
            List<T> connectors = new List<T>();
            Block<T> remainder = new Block<T>();
            foreach (T member in b)
            {
                if (!member.Equals(b.Connector))
                    remainder.AddMember(member);
            }

            HashSet<Block<T>> subBlocks = new HashSet<Block<T>>();
            while (remainder.Count > 0)
            {
                T newConnector = argMaxScore(b.Connector, remainder, scores);
                remainder.RemoveMember(newConnector);
                Block<T> cSet = getConnectableSet(newConnector, b.Connector, remainder, subBlocks, scores);
                connectors.Add(newConnector);
                subBlocks.Add(cSet);
            }
            return subBlocks;
        }

        private static T argMaxScore(T connector, Block<T> block, Dictionary<HashSet<T>, double> scores)
        {
            // finds the element e in block having greatest score with connector
            double maxScore = double.MinValue;
            T argMax = default(T);
            foreach (T e in block)
            {
                HashSet<T> h = new HashSet<T>() { connector, e };
                if (scores.ContainsKey(h))
                {
                    if (scores[h] > maxScore)
                    {
                        maxScore = scores[h];
                        argMax = e;
                    }
                }
            }
            return argMax;
        }

        private static Block<T> getConnectableSet(T connector, T parentConnector, Block<T> remainder, HashSet<Block<T>> otherBlocks, Dictionary<HashSet<T>, double> scores)
        {
            /* gets the block connectable through connector. Members are sought in remainder and otherBlocks, excluding their connectors. 
                Elements are moved from their original locations to the new block. */

            Block<T> connectableSet = new Block<T>(connector);
            foreach (T e in remainder.Members.ToArray())
            {
                bool skip = false;
                foreach (Block<T> otherBlock in otherBlocks)
                {
                    if (e.Equals(otherBlock.Connector))
                    {
                        skip = true;
                        break;
                    }
                }
                if (skip)
                    continue;

                HashSet<T> h = new HashSet<T>() { e, connector };
                if (!scores.ContainsKey(h))
                {
                    throw new Exception("Incomplete scores table.");
                }

                HashSet<T> p = new HashSet<T>() { e, parentConnector };
                if (!scores.ContainsKey(p))
                {
                    throw new Exception("Incomplete scores table.");
                }

                double score = scores[h];
                double pScore = scores[p];
                if (score > pScore)
                {
                    remainder.RemoveMember(e);
                    connectableSet.AddMember(e);
                }
            }
            foreach (Block<T> b in otherBlocks)
            {
                foreach (T e in b.Members.ToArray())
                {
                    // do not allow any of the other connectors to be used
                    if (e.Equals(b.Connector))
                    {
                        continue;
                    }

                    HashSet<T> h = new HashSet<T>() { e, connector };
                    if (!scores.ContainsKey(h))
                    {
                        throw new Exception("Incomplete scores table.");
                    }

                    HashSet<T> p = new HashSet<T>() { e, b.Connector };
                    if (!scores.ContainsKey(p))
                    {
                        throw new Exception("Incomplete scores table.");
                    }

                    double score = scores[h];
                    double pScore = scores[p];
                    if (score > pScore)
                    {
                        b.RemoveMember(e);
                        connectableSet.AddMember(e);
                    }
                }
            }
            return connectableSet;
        }

        public void AllocateCompatibilities(Dictionary<HashSet<T>, bool> compatibilities, Dictionary<T, bool> allocatedCompats)
        {
            AllocateCompatibilities(compatibilities, allocatedCompats, Tree);
        }

        private static void AllocateCompatibilities(Dictionary<HashSet<T>, bool> compatibilities, Dictionary<T, bool> allocatedCompats, Tree<Block<T>> tree)
        {
            foreach (Tree<Block<T>> child in tree.Children)
            {
                HashSet<T> hs = new HashSet<T>() { tree.Contents.Connector, child.Contents.Connector };
                if (compatibilities.ContainsKey(hs))
                {
                    allocatedCompats.Add(child.Contents.Connector, compatibilities[hs]);
                }
                else
                {
                    allocatedCompats.Add(child.Contents.Connector, false);
                }
                AllocateCompatibilities(compatibilities, allocatedCompats, child);
            }
        }

        public static Dictionary<string, Dictionary<string, double>> ExamineTracebacks(Tree<Block<string>> tree, Dictionary<HashSet<string>, double> scores)
        {
            Dictionary<string, Dictionary<string, double>> route = new Dictionary<string, Dictionary<string, double>>();
            List<Tree<Block<string>>> tips = new List<Tree<Block<string>>>();
            Tree<Block<string>>.GetTips(tree, tips);
            Tree<Block<string>>.GetInternalNodes(tree, ref tips);
            foreach (Tree<Block<string>> tip in tips)
            {
                route.Add(tip.Name, new Dictionary<string, double>());
                treeTrace(tip.Name, tip, scores, route);
            }
            return route;
        }

        private static void treeTrace(string tip, Tree<Block<string>> tree, Dictionary<HashSet<string>, double> scores, Dictionary<string, Dictionary<string, double>> route)
        {
            if (!route[tip].ContainsKey(tree.Name))
            {
                HashSet<string> hs = new HashSet<string>() { tip, tree.Name };
                if (scores.ContainsKey(hs))
                {
                    route[tip].Add(tree.Name, scores[hs]);
                }
                else
                {
                    route[tip].Add(tree.Name, double.NaN);
                }
            }

            if (tree.Parent != null)
            {
                treeTrace(tip, tree.Parent, scores, route);
            }
        }

    }

    public class SearchTreeAsymmetric<T> where T : IEquatable<T>
    {
        public HashSet<Block<T>> Blocks;
        public Tree<Block<T>> Tree;
        public Dictionary<T, Dictionary<T, double>> Scores;
        public int MaxBlockSize;

        public SearchTreeAsymmetric(IEnumerable<T> set, Dictionary<T, Dictionary<T, double>> scores)
        {
            T root = FindCenter(set, scores);
            Block<T> block = new Block<T>(root, set);
            Blocks = new HashSet<Block<T>>() { block };
            Scores = scores;
            Tree = new Tree<Block<T>>(root.ToString());
            Tree.Contents = block;
        }

        public int ComputeMaxBlockSize()
        {
            MaxBlockSize = 0;
            ComputeMaxBlockSize(Tree);
            return MaxBlockSize;
        }

        public void ComputeMaxBlockSize(Tree<Block<T>> tree)
        {
            if (tree.Children.Count == 0)
            {
                if (tree.Contents.Count > MaxBlockSize)
                {
                    MaxBlockSize = tree.Contents.Count;
                }
            }
            foreach (Tree<Block<T>> child in tree.Children)
            {
                ComputeMaxBlockSize(child);
            }
        }

        public void Refine()
        {
            Refine(Tree);
        }

        public void Refine(Tree<Block<T>> tree)
        {
            if (tree.Children.Count == 0)
            {
                if (tree.Contents.Count > 1)
                {
                    HashSet<Block<T>> h = SplitBlock(tree.Contents, Scores);
                    foreach (Block<T> block in h)
                    {
                        Tree<Block<T>> t = new Tree<Block<T>>(block.Connector.ToString());
                        t.BranchLength = 1;
                        t.Contents = block;
                        tree.AddChild(t);
                    }
                }
            }
            else
            {
                foreach (Tree<Block<T>> child in tree.Children)
                {
                    Refine(child);
                }
            }
        }

        public static T FindCenter(IEnumerable<T> set, Dictionary<T, Dictionary<T, double>> scores)
        {
            // maximize the minimum score over all members of the set
            Dictionary<T, double> minScores = new Dictionary<T, double>();
            foreach (T e0 in set)
            {
                double minScore = double.MaxValue;
                foreach (T e1 in set)
                {
                    if (e0.Equals(e1))
                        continue;

                    if (scores[e0][e1] < minScore)
                    {
                        minScore = scores[e0][e1];
                    }
                }
                minScores.Add(e0, minScore);
            }

            double maxMinScore = double.MinValue;
            T center = default(T);
            foreach (T e in minScores.Keys)
            {
                if (minScores[e] > maxMinScore)
                {
                    maxMinScore = minScores[e];
                    center = e;
                }
            }
            return center;
        }

        private static HashSet<Block<T>> SplitBlock(Block<T> b, Dictionary<T, Dictionary<T, double>> scores)
        {
            /* See Cloanalyst Techinical Details, section SearchTree/SplitBlock.
            Briefly, splits b into a set of blocks such that the connectors in each block are as close as possible to b.Connector and 
            each the mebers of each block are connectable to b.Connector through its own block's connector.
            */
            List<T> connectors = new List<T>();
            Block<T> remainder = new Block<T>();
            foreach (T member in b)
            {
                if (!member.Equals(b.Connector))
                    remainder.AddMember(member);
            }

            HashSet<Block<T>> subBlocks = new HashSet<Block<T>>();
            while (remainder.Count > 0)
            {
                T newConnector = argMaxScore(b.Connector, remainder, scores);
                remainder.RemoveMember(newConnector);
                Block<T> cSet = getConnectableSet(newConnector, b.Connector, remainder, subBlocks, scores);
                connectors.Add(newConnector);
                subBlocks.Add(cSet);
            }
            return subBlocks;
        }

        private static T argMaxScore(T connector, Block<T> block, Dictionary<T, Dictionary<T, double>> scores)
        {
            // finds the element e in block having greatest score with connector
            double maxScore = double.MinValue;
            T argMax = default(T);
            foreach (T e in block)
            {
                if (!scores.ContainsKey(connector) || !scores[connector].ContainsKey(e))
                {
                    throw new Exception("Incomplete scores table.");
                }

                if (scores[connector][e] > maxScore)
                {
                    maxScore = scores[connector][e];
                    argMax = e;
                }
            }
            return argMax;
        }

        private static Block<T> getConnectableSet(T connector, T parentConnector, Block<T> remainder, HashSet<Block<T>> otherBlocks, Dictionary<T, Dictionary<T, double>> scores)
        {
            /* gets the block connectable through connector. Members are sought in remainder and otherBlocks, excluding their connectors. 
                Elements are moved from their original locations to the new block. */

            Block<T> connectableSet = new Block<T>(connector);
            foreach (T e in remainder.Members.ToArray())
            {
                bool skip = false;
                foreach (Block<T> otherBlock in otherBlocks)
                {
                    if (e.Equals(otherBlock.Connector))
                    {
                        skip = true;
                        break;
                    }
                }
                if (skip)
                    continue;

                HashSet<T> h = new HashSet<T>() { e, connector };

                if (!scores.ContainsKey(connector) || !scores[connector].ContainsKey(e))
                {
                    throw new Exception("Incomplete scores table.");
                }

                if (!scores.ContainsKey(parentConnector) || !scores[parentConnector].ContainsKey(e))
                {
                    throw new Exception("Incomplete scores table.");
                }

                double score = scores[connector][e];
                double pScore = scores[parentConnector][e];
                if (score > pScore)
                {
                    remainder.RemoveMember(e);
                    connectableSet.AddMember(e);
                }
            }
            foreach (Block<T> b in otherBlocks)
            {
                foreach (T e in b.Members.ToArray())
                {
                    // do not allow any of the other connectors to be used
                    if (e.Equals(b.Connector))
                    {
                        continue;
                    }

                    if (!scores.ContainsKey(connector) || !scores[connector].ContainsKey(e))
                    {
                        throw new Exception("Incomplete scores table.");
                    }

                    if (!scores.ContainsKey(b.Connector) || !scores[b.Connector].ContainsKey(e))
                    {
                        throw new Exception("Incomplete scores table.");
                    }

                    double score = scores[connector][e];
                    double pScore = scores[b.Connector][e];
                    if (score > pScore)
                    {
                        b.RemoveMember(e);
                        connectableSet.AddMember(e);
                    }
                }
            }
            return connectableSet;
        }

        public void AllocateCompatibilities(Dictionary<T, Dictionary<T, bool>> compatibilities, Dictionary<T, bool> allocatedCompats)
        {
            AllocateCompatibilities(compatibilities, allocatedCompats, Tree);
        }

        private static void AllocateCompatibilities(Dictionary<T, Dictionary<T, bool>> compatibilities, Dictionary<T, bool> allocatedCompats, Tree<Block<T>> tree)
        {
            foreach (Tree<Block<T>> child in tree.Children)
            {
                HashSet<T> hs = new HashSet<T>() { tree.Contents.Connector, child.Contents.Connector };
                if (compatibilities.ContainsKey(tree.Contents.Connector) && compatibilities[tree.Contents.Connector].ContainsKey(child.Contents.Connector))
                {
                    allocatedCompats.Add(child.Contents.Connector, compatibilities[tree.Contents.Connector][child.Contents.Connector]);
                }
                else
                {
                    allocatedCompats.Add(child.Contents.Connector, false);
                }
                AllocateCompatibilities(compatibilities, allocatedCompats, child);
            }
        }

        public static Dictionary<string, Dictionary<string, double>> ExamineTracebacks(Tree<Block<string>> tree, Dictionary<string, Dictionary<string, double>> scores)
        {
            Dictionary<string, Dictionary<string, double>> route = new Dictionary<string, Dictionary<string, double>>();
            List<Tree<Block<string>>> tips = new List<Tree<Block<string>>>();
            Tree<Block<string>>.GetTips(tree, tips);
            Tree<Block<string>>.GetInternalNodes(tree, ref tips);
            foreach (Tree<Block<string>> tip in tips)
            {
                route.Add(tip.Name, new Dictionary<string, double>());
                treeTrace(tip.Name, tip, scores, route);
            }
            return route;
        }

        private static void treeTrace(string tip, Tree<Block<string>> tree, Dictionary<string, Dictionary<string, double>> scores, Dictionary<string, Dictionary<string, double>> route)
        {
            if (!route[tip].ContainsKey(tree.Name))
            {
                HashSet<string> hs = new HashSet<string>() { tip, tree.Name };
                if (scores.ContainsKey(tree.Name) && scores[tip].ContainsKey(tip))
                {
                    route[tip].Add(tree.Name, scores[tree.Name][tip]);
                }
                else
                {
                    route[tip].Add(tree.Name, double.NaN);
                }
            }

            if (tree.Parent != null)
            {
                treeTrace(tip, tree.Parent, scores, route);
            }
        }

    }

    public class SetSplit<T> where T : IEquatable<T>
    {
        public HashSet<Block<T>> Blocks;
        public HashSet<T> Centers;

        public SetSplit(T t0, T t1)
        {
            Blocks = new HashSet<Block<T>>();
            Centers = new HashSet<T>();
            Centers.Add(t0);
            Blocks.Add(new Block<T>(t0));
            Centers.Add(t1);
            Blocks.Add(new Block<T>(t1));
        }

        public override int GetHashCode()
        {
            return Centers.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            return Equals((SetSplit<T>)obj);
        }

        public bool Equals(SetSplit<T> setSplit)
        {
            return Centers.SetEquals(setSplit.Centers);
        }

        public static IEqualityComparer<SetSplit<T>> CreateSetSplitComparer()
        {
            return new SetSplitComparer<T>();
        }

    }

    public class SetSplitComparer<T> : IEqualityComparer<SetSplit<T>> where T : IEquatable<T>
    {
        public bool Equals(SetSplit<T> x, SetSplit<T> y)
        {
            return x.Centers.SetEquals(y.Centers);
        }

        public int GetHashCode(SetSplit<T> split)
        {
            return split.Centers.ElementAt(0).GetHashCode() ^ split.Centers.ElementAt(1).GetHashCode();
        }
    }
}
