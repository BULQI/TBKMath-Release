using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBKMath
{
    /// <summary>
    /// Convenient container for information typically associated with a tree node.
    /// </summary>
    public struct NodeInfo
    {
        /// <summary>
        /// The newick string for the node's child tree.
        /// </summary>
        public string ChildrenNewick;
        /// <summary>
        /// A name assigned to the node.
        /// </summary>
        public string Name;
        /// <summary>
        /// The length of the branch from the node to its parent.
        /// </summary>
        public double BranchLength;
    }

    /// <summary>
    /// A struct for constructing newick instruction strings for building a tree.
    /// </summary>
    public struct TreeBuildInstruction
    {
        /// <summary>
        /// The recognized symbols for parsing a newick file:  '(', ')', ',', ';' 
        /// </summary>
        public char Token;
        /// <summary>
        /// A string for the name assigned to a node.
        /// </summary>
        public string Name;
        /// <summary>
        /// The name given to the ASCII 0 character (for convenience).
        /// </summary>
        public static char NULL = (char)0; // ASCII data

        /// <summary>
        /// Returns the next tree-building instruction in a partial newick string.
        /// </summary>
        /// <param name="descriptor">A newick string.</param>
        /// <param name="cursorPosition">The position of the cursor upon entrance. The cursor position will be 
        /// update upon exit.</param>
        /// <returns>The next instruction extracted from the input string.</returns>
        public static TreeBuildInstruction NextCommand(StringBuilder descriptor, int cursorPosition)
        {
            TreeBuildInstruction newCommand = new TreeBuildInstruction();

            int positionOfNextToken = descriptor.ToString().IndexOfAny(tokens, cursorPosition);
            if (positionOfNextToken < 0)
            {
                newCommand.Token = NULL;
                newCommand.Name = descriptor.ToString();
                return newCommand;
            }

            newCommand.Name = descriptor.ToString().Substring(cursorPosition, positionOfNextToken + 1);
            newCommand.Token = descriptor.ToString().Substring(cursorPosition, 1)[0];
            cursorPosition = positionOfNextToken + 1;
            return newCommand;
        }

        private static char[] tokens = new char[] { '(', ')', ',', ';' };
    }

    /// <summary>
    /// Represents a tree-structured graph recursively.
    /// </summary>
    /// <typeparam name="T">The type of the contents associated with each node of the tree.</typeparam>
    public class Tree<T>
    {
        /// <summary>
        /// The children trees of the this tree.
        /// </summary>
        public List<Tree<T>> Children;
        /// <summary>
        /// The parental tree of this tree.
        /// </summary>
        public Tree<T> Parent;
        /// <summary>
        /// The name assigned to the root node of this tree.
        /// </summary>
        public string Name;
        private double branchLength;
        /// <summary>
        /// The length of the branch from the root of this tree to its parent.
        /// </summary>
        public double BranchLength
        {
            get { return branchLength; }
            set { branchLength = value; }
        }

        public double Age()
        {
            double age = branchLength;
            if (Parent != null)
                age += Parent.Age();

            return age;
        }

        /// <summary>
        /// The Newick string describing this tree.
        /// </summary>
        public string Descriptor;

        /// <summary>
        /// The object associated with the root node of this tree.
        /// </summary>
        public T Contents;

        public Tree(string newick, Tree<T> parent = null)
        {
            Children = new List<Tree<T>>();
            Descriptor = newick;

            if (newick == "")
                return;

            // remove unnecessary semicolon if it is present
            if (newick.Last() == ';')
            {
                newick = newick.Substring(0, newick.Length - 1);
            }

            if (parent != null)
            {
                Parent = parent;
            }

            NodeInfo ni = ParseNewick(newick);
            Name = ni.Name;
            BranchLength = ni.BranchLength;

            if (ni.ChildrenNewick != "NULL")
            {
                List<string> childNewicks = ExtractChildNewicks(ni.ChildrenNewick);
                foreach (string n in childNewicks)
                {
                    Children.Add(new Tree<T>(n, this));
                }
            }

        }

        public void AddChild(Tree<T> child)
        {
            Children.Add(child);
            child.Parent = this;
        }

        public void RemoveChild(Tree<T> child)
        {
            Children.Remove(child);
        }

        private static int findMatchingRightParenthesis(string text, int startPosition)
        {
            if (text[startPosition] != '(')
            {
                throw new ArgumentException("The character at startPosition is not a left parenthesis.");
            }
            int count = 0;
            for (int i = startPosition; i < text.Length; i++)
            {
                if (text[i] == '(')
                {
                    count++;
                }
                else if (text[i] == ')')
                {
                    count--;
                }

                if (count == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        private static NodeInfo parseName(string label)
        {
            NodeInfo ni = new NodeInfo();
            string[] tempString = label.Split(':');
            ni.Name = tempString[0];
            if (tempString.Count() > 1)
            {
                if (!double.TryParse(tempString[1], out ni.BranchLength))
                {
                    ni.BranchLength = double.NaN;
                }
            }
            else
            {
                ni.BranchLength = double.NaN;
            }
            return ni;
        }

        private static List<string> ExtractChildNewicks(string newick)
        {
            List<string> childNewicks = new List<string>();

            int level = 0;
            int start = 0;
            for (int i = 0; i < newick.Length; i++)
            {
                if (newick[i] == ',')
                {
                    if (level == 0)
                    {
                        childNewicks.Add(newick.Substring(start, i - start));
                        start = i + 1;
                    }
                }
                else if (newick[i] == '(')
                {
                    level++;
                }
                else if (newick[i] == ')')
                {
                    level--;
                }

            }
            childNewicks.Add(newick.Substring(start, newick.Length - start));
            return childNewicks;
        }

        private static NodeInfo ParseNewick(string newick)
        {
            string nameandlength;
            int lPos = -1;
            int rPos = -1;
            if (newick[0] == '(')
            {
                lPos = 0;
                rPos = findMatchingRightParenthesis(newick, 0);
                if (rPos < 1)
                {
                    throw new FormatException("Newick string is malformed: " + newick);
                }
                if (rPos < newick.Length - 1)
                {
                    nameandlength = newick.Substring(rPos + 1);
                }
                else
                {
                    nameandlength = string.Empty;
                }
            }
            else
            {
                nameandlength = newick;
            }

            NodeInfo ni = parseName(nameandlength);

            if (lPos > -1)
            {
                ni.ChildrenNewick = newick.Substring(1, rPos - 1);
            }
            else
            {
                ni.ChildrenNewick = "NULL";
            }

            return ni;
        }

        /// <summary>
        /// Performs rescaling of the lengths of all the branches in this tree.
        /// </summary>
        /// <param name="length">The length of the sequence contained as content in the root node.</param>
        /// <param name="alpha">Alpha in the equation NewBranchLength = (gamma * branchLength * length + alpha) / (length + beta)</param>
        /// <param name="beta">Beta in the equation NewBranchLength = (gamma * branchLength * length + alpha) / (length + beta)</param>
        /// <param name="gamma">Gamma in the equation NewBranchLength = (gamma * branchLength * length + alpha) / (length + beta)</param>
        public void RescaleBranchLength(int length, double alpha, double beta, double gamma)
        {
            foreach (Tree<T> child in Children)
            {
                child.RescaleBranchLength(length, alpha, beta, gamma);
            }
            branchLength = (gamma * branchLength * length + alpha) / (length + beta);
        }

        /// <summary>
        /// Computes the Newick descriptor for the string. This descriptor is computed by traversing the tree, 
        /// and does not refer to the tree's Descriptor itself.
        /// </summary>
        /// <param name="tree">The tree to serialize.</param>
        /// <returns>A Newick string containing the tree's description.</returns>
        public static string GetDescriptor(Tree<T> tree)
        {
            string desc = string.Empty;
            if (tree.Children.Count > 0)
            {
                desc += "(";
                foreach (Tree<T> child in tree.Children)
                {
                    desc += GetDescriptor(child) + ",";
                }
                desc = desc.Remove(desc.Length - 1) + ")";
            }

            desc += tree.Name;
            if (!double.IsNaN(tree.BranchLength))
            {
                desc += ":" + tree.BranchLength.ToString();
            }

            return desc;
        }

        public string GetDescriptor()
        {
            return GetDescriptor(this);
        }

        /// <summary>
        /// Computes the total number of nodes in a tree.
        /// </summary>
        /// <param name="tree">The tree whose nodes are to be counted.</param>
        /// <returns>The total number of nodes in the tree.</returns>
        public static int CountNodes(Tree<T> tree)
        {
            int returnVal = 1;
            foreach (Tree<T> child in tree.Children)
            {
                returnVal += CountNodes(child);
            }
            return returnVal;
        }

        public static int CountOccupiedNodes(Tree<T> tree)
        {
            int numOccupied = tree.Contents != null ? 1 : 0;
            foreach (Tree<T> child in tree.Children)
            {
                numOccupied += CountOccupiedNodes(child);
            }
            return numOccupied;
        }

        public static bool AllNodesOccupied(Tree<T> tree)
        {
            bool occupied = tree.Contents != null;
            foreach (Tree<T> child in tree.Children)
            {
                occupied &= AllNodesOccupied(child);
            }
            return occupied;
        }

        /// <summary>
        /// Counts the total number of leaves, or nodes without children, in a tree.
        /// </summary>
        /// <param name="tree">The tree whose leaves are to be counted.</param>
        /// <returns>The total number of leaves, or nodes without children.</returns>
        public static int CountLeaves(Tree<T> tree)
        {
            int returnVal = tree.Children.Count == 0 ? 1 : 0;
            foreach (Tree<T> child in tree.Children)
            {
                returnVal += CountLeaves(child);
            }
            return returnVal;
        }

        /// <summary>
        /// Attaches this tree to another tree as its parent.
        /// </summary>
        /// <param name="p">The tree to be treated as the parent to this tree.</param>
        public void SetParent(Tree<T> p, double distance = 0)
        {
            Parent = p;
            branchLength = distance;
        }

        /// <summary>
        /// The number of nodes with non-null contents.
        /// </summary>
        public static int NBaubles = 0;

        /// <summary>
        /// Associates the nodes of a tree with items from a list of objects.
        /// </summary>
        /// <param name="t">The tree to be decorated.</param>
        /// <param name="baubles">The list of objects used to decorate the tree. The keys are 
        /// the names associated with nodes on the tree, and the values are the individual objects.</param>
        public static void Decorate(Tree<T> t, Dictionary<string, T> baubles)
        {
            foreach (Tree<T> child in t.Children)
            {
                Decorate(child, baubles);
            }

            foreach (KeyValuePair<string, T> kvp in baubles)
            {
                if (kvp.Key == t.Name)
                {
                    t.Contents = kvp.Value;
                    NBaubles++;
                }
            }
            return;
        }

        /// <summary>
        /// Finds all of the internal nodes of a tree and returns them in a list.
        /// </summary>
        /// <param name="t">The tree to be examined.</param>
        /// <param name="intNodes">A list comprising the internal nodes of the tree being examined.</param>
        public static void GetInternalNodes(Tree<T> t, ref List<Tree<T>> intNodes)
        {
            if (t.Children.Count == 0)
            {
                return;
            }

            foreach (Tree<T> child in t.Children)
            {
                GetInternalNodes(child, ref intNodes);
            }

            if (t.Parent == null)
            {
                return;
            }

            intNodes.Add(t);
        }

        public static void GetTips(Tree<T> tree, List<Tree<T>> tips)
        {
            foreach (Tree<T> child in tree.Children)
            {
                GetTips(child, tips);
            }
            if (tree.Children.Count == 0)
            {
                tips.Add(tree);
            }
        }

        public static Dictionary<string, T> CollectAllContents(Tree<T> tree, Dictionary<string, T> contents = null)
        {
            if (contents == null)
                contents = new Dictionary<string, T>();

            foreach (Tree<T> child in tree.Children)
            {
                CollectAllContents(child, contents);
            }
            contents.Add(tree.Name, tree.Contents);
            return contents;
        }

        public void RenameNodes(Dictionary<string, string> nameKey)
        {
            renameNodes(nameKey, this);
        }

        private static void renameNodes(Dictionary<string, string> nameKey, Tree<T> tree)
        {
            if (nameKey.ContainsKey(tree.Name))
            {
                tree.Name = nameKey[tree.Name];
            }
            foreach (Tree<T> child in tree.Children)
            {
                renameNodes(nameKey, child);
            }
        }

        public static void ReverseAncestry(Tree<T> tree, Tree<T> child)
        {
            if (child != null && !tree.Children.Contains(child))
                return;

            if (tree.Parent != null)
                ReverseAncestry(tree.Parent, tree);

            if (child != null)
            {
                tree.RemoveChild(child);
                child.Parent = null;
                tree.Parent = child;
                tree.BranchLength = child.BranchLength;
                child.AddChild(tree);
            }
        }

        public void RootHere()
        {
            if (Parent == null)
                return;

            ReverseAncestry(Parent, this);
        }

        public static Tree<T> Reroot(Tree<T> tree, string newRootName)
        {
            Tree<T> newTree = GetDescendant(tree, newRootName);
            newTree.RootHere();
            return newTree;
        }

        public static Tree<T> GetDescendant(Tree<T> tree, string name)
        {
            if (tree.Name == name)
                return tree;

            Tree<T> descendant = null;
            foreach (Tree<T> child in tree.Children)
            {
                descendant = GetDescendant(child, name);
                if (descendant != null)
                    return descendant;
            }
            return null;
        }

        public static Tree<List<string>> Condense(Tree<T> tree, double theta)
        {
            Tree<List<string>> newTree = new Tree<List<string>>(tree.Name);
            newTree.BranchLength = tree.BranchLength;
            newTree.Contents = new List<string>() { tree.Name };
            foreach (Tree<T> child in tree.Children)
            {
                Tree<List<string>> childTree = Condense(child, theta);
                if (child.BranchLength > theta)
                {
                    newTree.Children.Add(childTree);
                    childTree.Parent = newTree;
                    childTree.BranchLength = child.BranchLength;
                }
                else
                {
                    newTree.Contents.AddRange(childTree.Contents);
                    foreach (Tree<List<string>> grandchild in childTree.Children)
                    {
                        newTree.Children.Add(grandchild);
                        grandchild.BranchLength += child.BranchLength;
                        grandchild.Parent = newTree;
                    }
                }
            }
            return newTree;
        }

        public static Tree<int> CondenseAndCount(Tree<T> tree, double theta)
        {
            Tree<int> newTree = new Tree<int>(tree.Name);
            newTree.BranchLength = tree.BranchLength;
            newTree.Contents = tree.Children.Count == 0 ? 1 : 0;
            foreach (Tree<T> child in tree.Children)
            {
                Tree<int> childTree = CondenseAndCount(child, theta);
                if (child.BranchLength > theta)
                {
                    newTree.Children.Add(childTree);
                    childTree.Parent = newTree;
                    childTree.BranchLength = child.BranchLength;
                }
                else
                {
                    newTree.Contents += childTree.Contents;
                    foreach (Tree<int> grandchild in childTree.Children)
                    {
                        newTree.Children.Add(grandchild);
                        grandchild.BranchLength += child.BranchLength;
                        grandchild.Parent = newTree;
                    }
                }
            }
            return newTree;
        }
         
        public static void NameIntermediates(Tree<T> tree, ref int next)
        {
            if (tree.Name == null || tree.Name.Length == 0)
            {
                tree.Name = "I" + next.ToString();
                next++;
            }
            foreach (Tree<T> child in tree.Children)
            {
                NameIntermediates(child, ref next);
            }
        }

        public static void NameIntermediates2(Tree<T> tree, ref int next)
        {
            if (tree.Name == null || tree.Name.Length == 0)
            {
                tree.Name = "I" + next.ToString();
                next++;
            }
            else
            {
                tree.Name = tree.Name + ".I" + next.ToString();
                next++;
            }
            foreach (Tree<T> child in tree.Children)
            {
                NameIntermediates2(child, ref next);
            }
        }

    }
}
