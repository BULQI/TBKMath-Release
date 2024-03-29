﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace TBKMath
{
    /// <summary>
    /// A partition is a semipartition with the additional constraint of having no overlapping blocks.
    /// </summary>
    /// <typeparam name="T">The type of the elements in The Set.</typeparam>
    public class Partition<T> : Semipartition<T>
    {
        /// <summary>
        /// Gets the block to which the argument is assigned.
        /// </summary>
        /// <param name="element">The element to be located.</param>
        /// <returns>The block containing the element. Throws an ArgumentException if the element is not contained
        /// in any block.</returns>
        public new HashSet<T> Assignments(T element)
        {
            if (!assignments.ContainsKey(element))
            {
                throw new ArgumentException("Element not found.");
            }
            return assignments[element][0];
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Partition()  : base()
        {

        }

        /// <summary>
        /// Implements the constructor for the base semipartition class.
        /// </summary>
        /// <param name="elements">As in semipartition.</param>
        /// <param name="oneBlock">As in semipartition.</param>
        public Partition(List<T> elements, bool oneBlock = false) : base(elements, oneBlock)
        {
        
        }

        /// <summary>
        /// Implements the constructor for the base semipartition class.
        /// </summary>
        /// <param name="blocks">As in semipartition. Throws an ArgumentException if the blocks overlap.</param>
        public Partition(HashSet<HashSet<T>> blocks) : base(blocks)
        {
            foreach (KeyValuePair<T, List<HashSet<T>>> kvp in assignments)
            {
                if (kvp.Value.Count > 1)
                {
                    throw new ArgumentException("The initial blocks must not be overlapping.");
                }
            }
        }

        /// <summary>
        /// Removes an element from its current block and places it into the specified block.
        /// </summary>
        /// <param name="element">The element to be moved.</param>
        /// <param name="destination">The block to which the element is to be moved. If the destination is null,
        /// creates a new block and places the element into it.</param>
        public void Move(T element, HashSet<T> destination = null)
        {
            // throw an exception if the element does not exist
            if (!elements.Contains(element))
            {
                throw new ArgumentException("The element specified does not exist.");
            }

            // throw an exception if the destination is non-null and not among the current blocks
            if (destination != null && !blocks.Contains(destination))
            {
                throw new ArgumentOutOfRangeException("Destination block must be found among the current blocks.");
            }

            // if the destination is one greater than the maximum index, create a new block and add it to Blocks
            if (destination == null)
            {
                destination = new HashSet<T>();
                blocks.Add(destination);
            }
            
            // add the element to the destination block
            destination.Add(element);

            // remove element from the source block
            assignments[element][0].Remove(element);

            // update Assignments
            // make a copy of the new destination block
            // this is necessary because the block indexing will not be consistent before and after block removal
            if (assignments[element][0].Count == 0)
            {
                blocks.Remove(assignments[element][0]);
            }

            // add the corrected assignment
            assignments[element][0] = destination;
        }

        /// <summary>
        /// Adds a new element to the partition, placing it in the indicated block.
        /// </summary>
        /// <param name="element">The new element.</param>
        /// <param name="destination">The block in which to place the new element. If null, makes a new block.</param>
        public void AddElement(T element, HashSet<T> destination = null)
        {
            if (elements.Contains(element))
            {
                throw new ArgumentException("Element already exists.");
            }

            if (destination != null && !blocks.Contains(destination))
            {
                throw new ArgumentOutOfRangeException("Destination block must be found among the current blocks.");
            }

            elements.Add(element);
            if (destination != null)
            {
                destination.Add(element);
                assignments.Add(element, new List<HashSet<T>>());
                assignments[element].Add(destination);
            }
            else
            {
                destination = new HashSet<T>();
                destination.Add(element);
                assignments.Add(element, new List<HashSet<T>>());
                assignments[element].Add(destination);
                blocks.Add(destination);
            }
            TotalSize++;
        }

        /// <summary>
        /// Removes the indicated element. Updates blocks and assignments.
        /// </summary>
        /// <param name="element"></param>
        public void RemoveElement(T element)
        {
            if (!elements.Contains(element))
            {
                throw new ArgumentException("Element does not exist.");
            }

            elements.Remove(element);

            // remove the element from the block that contains it
            assignments[element][0].Remove(element);

            // if the block that contained the element is now empty, remove it
            if (assignments[element][0].Count == 0)
            {
                blocks.Remove(assignments[element][0]);
            }

            // remove the element from the assignments dictionary
            assignments.Remove(element);
            TotalSize--;
        }

        /// <summary>
        /// Adds new block and all of its elements as new elements.
        /// </summary>
        /// <param name="block">The block to add.</param>
        public new void AddBlock(HashSet<T> block)
        {
            if (blocks.Contains(block))
            {
                throw new ArgumentException("The block already exists.");
            }

            foreach (T element in block)
            {
                if (elements.Contains(element))
                {
                    throw new ArgumentException("The block being added contains elements that are already present in this partition.");
                }
            }

            blocks.Add(block);
            foreach (T element in block)
            {
                assignments.Add(element, new List<HashSet<T>>());
                assignments[element].Add(block);
                elements.Add(element);
            }
        }

        /// <summary>
        /// Creates a new partition from two existing partitions with no members in common.
        /// </summary>
        /// <param name="p1">A partition.</param>
        /// <param name="p2">A partition with no members in common with the first partition.</param>
        /// <returns></returns>
        public static Partition<T> Join(Partition<T> p1, Partition<T> p2)
        {
            foreach (T element in p1.elements) 
            {
                if (p2.elements.Contains(element))
                {
                    throw new ArgumentException("Partitions must have disjoint elements.");
                }
            }

            Partition<T> newPartition = new Partition<T>();
            foreach (HashSet<T> block in p1.blocks)
            {
                newPartition.AddBlock(block);
            }
            foreach (HashSet<T> block in p2.blocks)
            {
                newPartition.AddBlock(block);
            }

            return newPartition;
        }

        /// <summary>
        /// Serializes the partition.
        /// </summary>
        /// <returns>A comma-delimited string with curly-bracket delimited sets.</returns>
        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.Append('{');
            foreach (HashSet<T> block in blocks)
            {
                s.Append('{');
                foreach (T element in block)
                {
                    s.Append(element.ToString() + ",");
                }
                s.Remove(s.Length - 1, 1);
                s.Append("},");
            }
            s.Remove(s.Length - 1, 1);
            s.Append('}');
            return s.ToString();
        }
    }

    /// <summary>
    /// A semipartition comprises a set (The Set) of elements, a set of blocks, and an assignment of elements to blocks.
    /// Each element must be assigned to at least one block, and may be assigned to more than one.
    /// </summary>
    /// <typeparam name="T">The base type for members of the set.</typeparam>
    public class Semipartition<T> : IEnumerable
    {
        /// <summary>
        /// The total number of elements in The Set.
        /// </summary>
        public int TotalSize;
        /// <summary>
        /// The elements contained in The Set.
        /// </summary>
        protected List<T> elements;
        /// <summary>
        /// The set of blocks.
        /// </summary>
        protected HashSet<HashSet<T>> blocks;
        /// <summary>
        /// The assignment of elements into blocks. Note that the assignment is not unique. Any element may be assigned to more than 
        /// one block, but must be assigned to at least one.
        /// </summary>
        protected Dictionary<T, List<HashSet<T>>> assignments;
        
        /// <summary>
        /// The number of blocks in the semipartition.
        /// </summary>
        public int NumberOfBlocks
        {
            get
            {
                return blocks.Count;
            }
        }

        /// <summary>
        /// The blocks as a set of blocks.
        /// </summary>
        public HashSet<HashSet<T>> Blocks
        {
            get { return blocks; }
        }

        /// <summary>
        /// The elements of The Set.
        /// </summary>
        public List<T> Elements
        {
            get { return elements; }
        }

        /// <summary>
        /// The assignment of elements into blocks. Note that the assignment is not unique. Any element may be assigned to more than 
        /// one block, but must be assigned to at least one.
        /// </summary>
        public List<HashSet<T>> Assignments(T element)
        {
            if (!assignments.ContainsKey(element))
            {
                throw new ArgumentException("Element not found.");
            }
            return assignments[element];
        }

        /// <summary>
        /// Constructor. Initializes the elements list, the assignments dictionary, and the blocks hashset.
        /// </summary>
        public Semipartition()
        {
            elements = new List<T>();
            TotalSize = 0;
            assignments = new Dictionary<T, List<HashSet<T>>>();
            blocks = new HashSet<HashSet<T>>();
        }

        /// <summary>
        /// Constructor. Initializes the elements list, the assignments dictionary, and the blocks hashset.
        /// </summary>
        /// <param name="elements">The elements that constitute the set, at least initially.</param>
        /// <param name="singleBlock">If true, places all elements into a single block. If false, places each
        /// element into its own block.</param>
        public Semipartition(List<T> elements, bool singleBlock = false)
        {
            this.elements = elements;
            TotalSize = elements.Count;
            this.blocks = new HashSet<HashSet<T>>();
            assignments = new Dictionary<T, List<HashSet<T>>>();
            // by default, place each element into its own block unless singleBlock
            if (singleBlock)
            {
                HashSet<T> oneblock = new HashSet<T>();
                blocks.Add(oneblock);
                foreach (T element in elements)
                {
                    oneblock.Add(element);
                    assignments.Add(element, new List<HashSet<T>>() { oneblock });
                }
            }
            else
            {
                foreach (T element in elements)
                {
                    HashSet<T> newblock = new HashSet<T>();
                    newblock.Add(element);
                    blocks.Add(newblock);
                    assignments.Add(element, new List<HashSet<T>>() { newblock });
                }
            }
            PurgeSubblocks();
        }

        /// <summary>
        /// Constructor. Initializes the elements list, the assignments dictionary, and the blocks hashset.
        /// </summary>
        /// <param name="blocks">The blocks provided in the argument become the blocks of the semipartition. The union of the blocks
        /// is The Set.</param>
        public Semipartition(HashSet<HashSet<T>> blocks)
        {
            assignments = new Dictionary<T, List<HashSet<T>>>();
            this.blocks = blocks;
            elements = new List<T>();
            foreach (HashSet<T> block in blocks)
            {
                foreach (T element in block)
                {
                    if (!elements.Contains(element))
                    {
                        elements.Add(element);
                    }
                    if (!assignments.ContainsKey(element))
                    {
                        assignments.Add(element, new List<HashSet<T>>());
                    }
                    assignments[element].Add(block);
                }
            }
            PurgeSubblocks();
            TotalSize = elements.Count;
        }

        /// <summary>
        /// Implements the GetEnumerator method for the IEnumerable interface.
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return new BlockEnum<T>(blocks);
        }

        /// <summary>
        /// Places the union of all the blocks in the argument into a single block. Removes all the initial blocks, 
        /// retaining only the merged block.
        /// </summary>
        /// <param name="semipartition">The semipartition to be processed.</param>
        public static void MergeAllBlocks(Semipartition<T> semipartition)
        {
            semipartition.assignments = new Dictionary<T, List<HashSet<T>>>();
            semipartition.blocks = new HashSet<HashSet<T>>();
            HashSet<T> block = new HashSet<T>();
            foreach (T element in semipartition.elements)
            {
                block.Add(element);
                semipartition.assignments.Add(element, new List<HashSet<T>>() { block });
            }
            semipartition.blocks.Add(block);
        }

        /// <summary>
        /// Places the union of all the blocks in the instance into a single block. Removes all the initial blocks, 
        /// retaining only the merged block.
        /// </summary>
        public void MergeAllBlocks()
        {
            MergeAllBlocks(this);
        }

        /// <summary>
        /// Replaces a block with the partition of that block.
        /// </summary>
        /// <param name="block">The block to be partitioned.</param>
        /// <param name="blockPartition">The partition of the specified block.</param>
        public void PartitionBlock(HashSet<T> block, Partition<T> blockPartition)
        {
            if (!blocks.Contains(block))
            {
                throw new ArgumentException("The specified block is not in this semipartition.");
            }

            foreach (T element in block)
            {
                assignments[element].Remove(block);
                assignments[element].Add(blockPartition.Assignments(element));

                if (!blocks.Contains(blockPartition.Assignments(element)))
                {
                    blocks.Add(blockPartition.Assignments(element));
                }
            }
            blocks.Remove(block);
        }

        /// <summary>
        /// Removes from the partition any blocks completely contained within another block.
        /// </summary>
        /// <param name="semipartition">The semipartition to process.</param>
        public static void PurgeSubblocks(Semipartition<T> semipartition)
        {
            List<HashSet<T>> toRemove = new List<HashSet<T>>();
            foreach (HashSet<T> block1 in semipartition.blocks)
            {
                foreach (HashSet<T> block2 in semipartition.blocks)
                {
                    if (block1 == block2)
                        continue;

                    if (block1.IsSubsetOf(block2))
                    {
                        if (!toRemove.Contains(block1) && !toRemove.Contains(block2))
                            toRemove.Add(block1);
                        break;
                    }
                }
            }

            foreach (HashSet<T> block in toRemove)
            {
                foreach (T element in block)
                {
                    if (semipartition.assignments[element].Contains(block))
                    {
                        semipartition.assignments[element].Remove(block);
                    }
                }
                semipartition.blocks.Remove(block);
            }
        }

        /// <summary>
        /// Removes from the partition any blocks completely contained within another block.
        /// </summary>
        public void PurgeSubblocks()
        {
            PurgeSubblocks(this);
        }

        /// <summary>
        /// Finds the nonoverlapping blocks and places them into a partition 
        /// places the remaining overlapping blocks into a semipartition 
        /// </summary>
        /// <param name="semipartition">The semipartition to be processed.</param>
        /// <returns>A tuple containing a semipartition and a partition. The two have no elements in common, and
        /// every element in the original semipartition is in one or the other item of the tuple.</returns> 
        public static Tuple<Semipartition<T>, Partition<T>> Decompose(Semipartition<T> semipartition)
        {
            Semipartition<T> newSemipartition = new Semipartition<T>();
            Partition<T> partition = new Partition<T>();

            List<HashSet<T>> overlaps = new List<HashSet<T>>();
            foreach (HashSet<T> block1 in semipartition.blocks)
            {
                foreach (HashSet<T> block2 in semipartition.blocks)
                {
                    if (block1 == block2)
                        break;

                    if (block2.Overlaps(block1))
                    {
                        if (!overlaps.Contains(block1))
                            overlaps.Add(block1);
                        
                        if (!overlaps.Contains(block2))
                            overlaps.Add(block2);
                    }
                }
            }

            foreach (HashSet<T> block in semipartition.blocks)
            {
                if (overlaps.Contains(block))
                {
                    newSemipartition.AddBlock(block);
                }
                else
                {
                    partition.AddBlock(block);
                }
            }

            return new Tuple<Semipartition<T>, Partition<T>>(newSemipartition, partition);
        }

        /// <summary>
        /// Perform a decomposition on this instance.
        /// </summary>
        /// <returns>As above for the static method.</returns>
        public Tuple<Semipartition<T>, Partition<T>> Decompose()
        {
            return Decompose(this);
        }

        /// <summary>
        /// Finds all pairs of blocks with non-empty intersection and merges them. Iterated until no overlapping pairs remain.
        /// </summary>
        /// <param name="semipartition">The semipartition to be processed.</param>
        /// <returns>The resulting partition.</returns>
        public static Partition<T> MergeOverlappingBlocks(Semipartition<T> semipartition)
        {
            HashSet<HashSet<T>> blocks = new HashSet<HashSet<T>>();
            foreach (HashSet<T> block in semipartition.blocks)
            {
                HashSet<T> newBlock = new HashSet<T>();
                foreach (T element in block)
                {
                    newBlock.Add(element);
                }
                blocks.Add(newBlock);
            }

            int N = semipartition.blocks.Count;
            int newIndexStart = N;
            List<T> inoverlaps = new List<T>();
            for (int i = 0; i < N; i++)
            {
                for (int j = i + 1; j < N; j++)
                {
                    if (blocks.ElementAt(i).Overlaps(blocks.ElementAt(j)))
                    {
                        HashSet<T> newBlock = new HashSet<T>();
                        newBlock.UnionWith(blocks.ElementAt(i));
                        newBlock.UnionWith(blocks.ElementAt(j));
                        foreach (T element in newBlock)
                        {
                            if (!inoverlaps.Contains(element))
                            {
                                inoverlaps.Add(element);
                            }
                        }
                        // remove the higher-number block first
                        blocks.Remove(blocks.ElementAt(j));
                        blocks.Remove(blocks.ElementAt(i));
                        blocks.Add(newBlock);
                        // i stays the same (but is decremented here to do so), N is decremented by 1
                        N--;
                        i--;
                        break;
                    }
                }
            }

            Partition<T> partition = new Partition<T>();
            foreach (HashSet<T> block in blocks)
            {
                partition.AddBlock(block);
            }

            return partition;
        }

        /// <summary>
        /// Applies the static MergeOverlappingBlocks to this instance.
        /// </summary>
        /// <returns>As in the static method.</returns>
        public Partition<T> MergeOverlappingBlocks()
        {
            return MergeOverlappingBlocks(this);
        }

        /// <summary>
        /// Checks to see if any pair of blocks has non-null intersection.
        /// </summary>
        /// <param name="semipartition">The semipartition to be examined.</param>
        /// <returns>True if any pair of blocks has non-null intersection. False otherwise.</returns>
        public static bool ContainsOverlappingBlocks(Semipartition<T> semipartition)
        {
            foreach (HashSet<T> block1 in semipartition)
            {
                foreach (HashSet<T> block2 in semipartition)
                {
                    if (block1 == block2)
                        break;

                    if (block1.Overlaps(block2))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Adds block to the set of blocks. Updates members and assignments as necessary.
        /// </summary>
        /// <param name="block">The block to be added.</param>
        public void AddBlock(HashSet<T> block)
        {
            blocks.Add(block);
            foreach (T element in block)
            {
                if (!assignments.ContainsKey(element))
                {
                    assignments.Add(element, new List<HashSet<T>>());
                }
                assignments[element].Add(block);
                if (!elements.Contains(element))
                {
                    elements.Add(element);
                }
            }
            PurgeSubblocks();
        }

        /// <summary>
        /// Serializes the semipartition. Relies on the ToString() method for elements.
        /// </summary>
        /// <returns>A comma-delitimed string containing curly-bracket-delimited sets.</returns>
        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.Append('{');
            foreach (HashSet<T> block in blocks)
            {
                s.Append('{');
                foreach (T element in block)
                {
                    s.Append(element.ToString() + ",");
                }
                s.Remove(s.Length - 1, 1);
                s.Append("},");
            }
            s.Remove(s.Length - 1, 1);
            s.Append('}');
            return s.ToString();
        }

        /// <summary>
        /// Converts the semipartition to a table. Relies on the ToString() method for the elements.
        /// </summary>
        /// <returns>A List of lists of type string. Each row in the table starts with a block index.</returns>
        public List<List<string>> ToTable()
        {
            List<List<string>> table = new List<List<string>>();

            Dictionary<HashSet<T>, int> blockIndex = new Dictionary<HashSet<T>,int>();
            int index = 1;
            foreach (HashSet<T> block in blocks)
            {
                blockIndex.Add(block, index);
                index++;
            }

            foreach (KeyValuePair<T,List<HashSet<T>>> kvp in assignments)
            {
                List<string> row = new List<string>() { kvp.Key.ToString() };
                foreach (HashSet<T> block in kvp.Value)
                {
                    row.Add(blockIndex[block].ToString());
                }
                table.Add(row);
            }
            return table;
        }
    }

    /// <summary>
    /// A delegate to a function that computes a score over elements in any set of elements.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the set.</typeparam>
    /// <param name="set">The set of elements that serves as argument to the function.</param>
    /// <returns></returns>
    public delegate double GetScore<T>(HashSet<T> set);

    /// <summary>
    /// Provides an enumerator so that the foreach construction can be used with Partition and Semipartition blocks.
    /// </summary>
    public class BlockEnum<T> : IEnumerator
    {
        private HashSet<HashSet<T>>.Enumerator enumerator;
        private HashSet<HashSet<T>> members;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="_members">Pass the members field of the PolynucCollection being enumerated.</param>
        public BlockEnum(HashSet<HashSet<T>> _members)
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
        public void Reset() 
        {
            throw new NotImplementedException();
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
        public HashSet<T> Current
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

    //public class PartitionProcess<T>
    //{
    //    // implements a random process moving from partitions to partitions using elementary moves
    //    Dictionary<HashSet<T>, double> scores;
    //    private Partition<T> partition;
    //    public GetScore<T> GetScore;
    //    public double Beta;
    //    private Troschuetz.Random.Generators.MT19937Generator gen;
    //    private Troschuetz.Random.Distributions.Discrete.DiscreteUniformDistribution dud;
    //    private Troschuetz.Random.Distributions.Continuous.ContinuousUniformDistribution cud;

    //    T subject;
    //    int destinationIndex;
    //    double deltaScore;

    //    public double Score;

    //    public PartitionProcess(Partition<T> _partition, GetScore<T> getScoreFunction)
    //    {
    //        partition = _partition;
    //        scores = new Dictionary<HashSet<T>, double>(HashSet<T>.CreateSetComparer());
    //        gen = new Troschuetz.Random.Generators.MT19937Generator();
    //        dud = new Troschuetz.Random.Distributions.Discrete.DiscreteUniformDistribution(gen);
    //        dud.Alpha = 0;
    //        dud.Beta = partition.TotalSize - 1;
    //        Beta = 1;
    //        cud = new Troschuetz.Random.Distributions.Continuous.ContinuousUniformDistribution(gen);
    //        cud.Alpha = 0;
    //        cud.Beta = 1.0;
    //        GetScore = getScoreFunction;
    //        foreach (HashSet<T> block in partition.Blocks)
    //        {
    //            double s = getScore(block);
    //            Score += s;
    //        }
    //        destinationIndex = -1;
    //        deltaScore = double.NaN;
    //    }

    //    public void Step()
    //    {
    //        // select element to move, call it "subject"
    //        // choose with uniform probability on [0,totalSize-1]
    //        dud.Beta = partition.TotalSize - 1;
    //        int elementindex = dud.Next();
    //        subject = partition.Elements[elementindex];

    //        // obtain the block in which the subject resides
    //        HashSet<T> oldSourceBlock = partition.Assignments(subject);

    //        // prepare a new block representing the subject's block without the subject
    //        // add the elements from the oldblock to the new block
    //        HashSet<T> newSourceBlock = new HashSet<T>();
    //        foreach (T element in oldSourceBlock)
    //        {
    //            newSourceBlock.Add(element);
    //        }
    //        // and take the subject away
    //        newSourceBlock.Remove(subject);

    //        // create old and new versions of the destination block
    //        HashSet<T> oldDestinationBlock;
    //        HashSet<T> newDestinationBlock;

    //        // now choose a block at random with uniform probablity on [0, numberOfBlock - ]
    //        dud.Beta = partition.NumberOfBlocks - 1;
    //        destinationIndex = dud.Next();

    //        // check to see if the destination block is the same as the source block. If it is 
    //        // if the source block is a singleton, choose another index, else make a new block
    //        if (partition.Blocks.ElementAt(destinationIndex)==oldSourceBlock)
    //        {
    //            if (oldSourceBlock.Count == 1)
    //            {
    //                // it's not a real move
    //                // so generate a new random integer on [1,numberOfBlock - 1], use sum of this number and destinationIndex mod numberOfBlocks
    //                dud.Alpha = 1;
    //                destinationIndex = (destinationIndex + dud.Next()) % partition.NumberOfBlocks;
    //                oldDestinationBlock = partition.Blocks.ElementAt(destinationIndex);
    //                dud.Alpha = 0;
    //            }
    //            else
    //            {
    //                oldDestinationBlock = new HashSet<T>();
    //                destinationIndex = partition.NumberOfBlocks;
    //            }
    //        }
    //        else
    //        {
    //            oldDestinationBlock = partition.Blocks.ElementAt(destinationIndex);
    //        }

    //        // copy the elements from the old destination block to the new one
    //        // and then add the subject as well
    //        newDestinationBlock = new HashSet<T>();
    //        foreach (T element in oldDestinationBlock)
    //        {
    //            newDestinationBlock.Add(element);
    //        }
    //        newDestinationBlock.Add(subject);
            
    //        // compute the scores  before and after the moves, and their difference
    //        double oldScore = getScore(oldSourceBlock) + getScore(oldDestinationBlock);
    //        double newScore = getScore(newSourceBlock) + getScore(newDestinationBlock); 
    //        deltaScore = newScore - oldScore;

    //        // convert to a probability and evaluate the acceptance criterion
    //        double p = Math.Exp(deltaScore);
    //        bool accept = false;
    //        if (p >= 1)
    //        {
    //            accept = true;
    //        }
    //        else
    //        {
    //            double u = cud.NextDouble();
    //            if (u < Math.Pow(p, Beta))
    //            {
    //                accept = true;
    //            }
    //        }

    //        // the proposed move is accepted, update the Score and instruct the partition to move the subject as indicated
    //        if (accept)
    //        {
    //            partition.Move(subject, destinationIndex);
    //            Score += deltaScore;
    //        }
    //    }

    //    private double getScore(HashSet<T> block)
    //    {
    //        if (!scores.ContainsKey(block))
    //        {
    //            double s = GetScore(block);
    //            scores.Add(block, s);
    //        }
    //        return scores[block];
    //    }

    //    public override string ToString()
    //    {
    //        return Score.ToString() + "\t" + (subject != null ? subject.ToString() : "null") + "\t" + destinationIndex.ToString() + "\t" + deltaScore.ToString() + "\t" + partition.ToString();
    //    }

    //    public List<string>[] ToTable()
    //    {
    //        return partition.ToTable();
    //    }

    //}

    public class GreedyAggregator<T>
    {
        // implements a clustering by aggregation process
        Dictionary<HashSet<T>, double> scores;
        private Partition<T> partition;
        public GetScore<T> GetScore;
        Dictionary<T, double> DeltaScores;
        public List<T> Elements;

        int destinationIndex;
        double deltaScore;

        public double Score;

        public HashSet<HashSet<T>> Blocks
        {
            get { return partition.Blocks; }
        }

        public GreedyAggregator(GetScore<T> getScore)
        {
            partition = new Partition<T>(new List<T>());
            Elements = new List<T>();
            scores = new Dictionary<HashSet<T>, double>(HashSet<T>.CreateSetComparer());
            GetScore = getScore;
            destinationIndex = -1;
            deltaScore = double.NaN;
            DeltaScores = new Dictionary<T, double>();
        }

        public bool Step(T subject)
        {
            // make the default to add a singleton in case anything fails in the computation
            destinationIndex = -1;

            Elements.Add(subject);
            HashSet<T> singleton = new HashSet<T>();
            singleton.Add(subject);
            double oldScore1 = getScore(singleton);
            if (double.IsNegativeInfinity(oldScore1))
            {
                return false;
            }

            // loop over all blocks in the partition, compute the score, mark the one with the best score.
            deltaScore = double.MinValue;
            
            for (int j = 0; j < partition.Blocks.Count; j++)
            {
                HashSet<T> block = partition.Blocks.ElementAt(j);
                double oldScore2 = getScore(block);
                double oldScore = oldScore1 + oldScore2;
                block.Add(subject);

                double newScore = getScore(block);
                double dScore = newScore - oldScore;
                if (dScore > deltaScore)
                {
                    deltaScore = dScore;
                    destinationIndex = j;
                }
                block.Remove(subject);
            }
            HashSet<T> empty = new HashSet<T>();
            double oldscore2 = getScore(empty);
            double oldscore = oldScore1 + oldscore2;
            empty.Add(subject);

            double newscore = getScore(empty);
            double dscore = newscore - oldscore;
            if (dscore > deltaScore)
            {
                destinationIndex = -1;
            }

            DeltaScores.Add(subject, deltaScore);
            if (destinationIndex < 0)
            {
                partition.AddElement(subject);
            }
            else
            {
                partition.AddElement(subject, partition.Blocks.ElementAt(destinationIndex));
            }
            Score += oldScore1 + deltaScore;
            return true;
        }

        public bool FindMerge(HashSet<T> subject, double minScore = 0)
        {
            // make the default to add a singleton in case anything fails in the computation
            destinationIndex = -1;

            double oldScore1 = getScore(subject);
            if (double.IsNegativeInfinity(oldScore1))
            {
                return false;
            }

            // loop over all blocks in the partition, compute the score, mark the one with the best score.
            deltaScore = double.MinValue;

            for (int j = 0; j < partition.Blocks.Count; j++)
            {
                HashSet<T> block = partition.Blocks.ElementAt(j);
                if (block == subject)
                    continue;

                double oldScore2 = getScore(block);
                double oldScore = oldScore1 + oldScore2;
                foreach (T item in subject)
                {
                    block.Add(item);
                }

                double newScore = getScore(block);
                double dScore = newScore - oldScore;
                if (dScore > deltaScore)
                {
                    deltaScore = dScore;
                    destinationIndex = j;
                }
                foreach (T item in subject)
                {
                    block.Remove(item);
                }
            }

            if (deltaScore > minScore)
            {
                foreach (T item in subject)
                {
                    partition.Blocks.ElementAt(destinationIndex).Add(item);
                }
                partition.Blocks.Remove(subject);
                Score += oldScore1 + deltaScore;
                return true;
            }
            else
            {
                return false;
            }
        }

        private double getScore(HashSet<T> block)
        {
            if (!scores.ContainsKey(block))
            {
                double s = GetScore(block);
                scores.Add(block, s);
            }
            return scores[block];
        }

        public override string ToString()
        {
            return Score.ToString() + "\t" + partition.ToString();
        }

        public List<string>[] ToTable()
        {
            List<string>[] table = new List<string>[partition.Elements.Count];

            int index = 0;
            for (int i = 0; i < partition.Blocks.Count; i++)
            {
                HashSet<T> block = partition.Blocks.ElementAt(i);
                int blocksize = block.Count;
                for (int j = 0; j < blocksize; j++)
                {
                    table[index] = new List<string>();
                    T element = block.ElementAt(j);
                    // the string representation of the element
                    table[index].Add(element.ToString());
                    // the block number
                    table[index].Add(i.ToString());
                    // the block score
                    table[index].Add(scores[block].ToString());
                    // the element delta score
                    table[index].Add(DeltaScores[element].ToString());

                    index++;
                }
            }
            return table;
        }

    }
}



