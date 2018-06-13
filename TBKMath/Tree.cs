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

        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="src"></param>
        public Tree(Tree<T> src)
        {
            Name = src.Name;
            branchLength = src.branchLength;
            Descriptor = src.Descriptor;
            Contents = src.Contents;
            if (src.Children != null)
            {
                this.Children = new List<Tree<T>>(src.Children.Count);
                foreach (var child in src.Children)
                {
                    var kid = new Tree<T>(child);
                    kid.Parent = this;
                    this.Children.Add(kid);
                }
            }
        }

        public static Tree<T> ChangeContentType<U>(Tree<U> src, TConverter<T, U> converter)
        {
            Tree<T> t = new Tree<T>("");
            t.Name = src.Name;
            t.branchLength = src.branchLength;
            t.Descriptor = src.Descriptor;
            if (src.Contents != null)
            {
                t.Contents = converter.Convert(src.Contents);
            }
            if (src.Children != null)
            {
                t.Children = new List<Tree<T>>(src.Children.Count);
                foreach (Tree<U> child in src.Children)
                {
                    Tree<T> kid = ChangeContentType(child, converter);
                    kid.Parent = t;
                    t.Children.Add(kid);
                }
            }
            return t;
        }
        
        /// <summary>
        /// get distance from root to the node with given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public double GetDistanceToNode(string name)
        {
            if (this.Name == name) return 0;
            foreach (var child in Children)
            {
                var d = child.GetDistanceHelper(name);
                if (d != double.MinValue) return d;
            }
            throw new Exception("target node not found exception");
        }


        /// <summary>
        /// get path from root to the node with given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string[] GetPath(string name)
        {
            var pathstr = GetPathHelper(name);
            if (pathstr == null)
            {
                throw new Exception("target node not found exception");
            }
            return pathstr.Split(',');
        }

        /// <summary>
        /// helps to find path
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string GetPathHelper(string name)
        {
            if (this.Name == name)
            {
                return name;
            }
            foreach (var child in Children)
            {
                var kidpath = child.GetPathHelper(name);
                if (kidpath != null)
                {
                    return this.Name + "," + kidpath;
                }
            }
            return null;
        }


        /// <summary>
        /// recursively find node to compute distance
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private double GetDistanceHelper(string name)
        {
            if (this.Name == name)
            {
                return this.branchLength;
            }
            foreach (var child in this.Children)
            {
                var d = child.GetDistanceHelper(name);
                if (d != double.MinValue)
                {
                    return d + this.branchLength;
                }
            }
            return double.MinValue;
        }

        /// <summary>
        /// return names for all nodes.
        /// </summary>
        /// <returns></returns>
        internal List<String> getNodeNames()
        {
            var trlist = findSubTree();
            return trlist.Select(x => x.Name).ToList();
        }

        /// <summary>
        /// return only leaf nodes
        /// </summary>
        public List<string> getLeafNodeNames()
        {
            var trlist = findSubTree();
            return trlist.Where(y => y.Children.Count == 0).Select(x => x.Name).ToList();
        }

        public List<string> getInternalNodeNames()
        {
            var trlist = findSubTree();
            return trlist.Where(y => y.Children.Count > 0).Select(x => x.Name).ToList();
        }

        /// <summary>
        /// find all nodes (tree)
        /// </summary>
        /// <returns></returns>
        private List<Tree<T>> findSubTree()
        {
            List<Tree<T>> list = new List<Tree<T>>();
            findSubTree(list);
            return list;
        }

        /// <summary>
        /// find all nodes helper
        /// </summary>
        /// <param name="trlist"></param>
        private void findSubTree(List<Tree<T>> trlist)
        {
            trlist.Add(this);
            foreach (var child in Children)
            {
                child.findSubTree(trlist);
            }
        }

        /// <summary>
        /// return dist matrix in indexed format.
        /// first dimension for leave nodes
        /// second dimension is internal nodes.
        /// </summary>
        /// <returns></returns>
        public double[][] GetDistanceMatrix()
        {
            List<Tree<T>> trlist = findSubTree();
            //List<Tree<T>> leaves = trlist.Where(x => x.Children.Count == 0).ToList();
            //List<Tree<T>> branches = trlist.Where(x => x.Children.Count > 0).ToList();

            var leaves = new List<Tree<T>>();
            var branches = new List<Tree<T>>();
            foreach (var tree in trlist)
            {
                if (tree.Children.Count > 0)
                {
                    branches.Add(tree);
                }
                else
                {
                    leaves.Add(tree);
                }
            }

            double[][] distMatrix = new double[leaves.Count][];
            for (int i = 0; i < leaves.Count; i++)
            {
                distMatrix[i] = new double[branches.Count];
            }

            for (int i = 0; i < leaves.Count; i++)
            {
                for (int j = 0; j < branches.Count; j++)
                {
                    var d = findDistance(leaves[i], branches[j]);
                    distMatrix[i][j] = d;
                }
            }
            return distMatrix;
        }

        /// <summary>
        /// find lowest common accessor of the two nodes.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        Tree<T> findLCA(Tree<T> x, Tree<T> y)
        {
            if (x == this || y == this) return this;
            Tree<T> a = null;
            Tree<T> b = null;
            foreach (var child in Children)
            {
                var lca = child.findLCA(x, y);
                if (lca == null) continue;
                if (a == null) a = lca;
                else
                {
                    b = lca;
                    break;
                }
            }
            if (a != null && b != null) return this;
            return a ?? b;
        }

        /// <summary>
        /// find distance between tree x and y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        double findDistance(Tree<T> x, Tree<T> y)
        {
            var lca = findLCA(x, y);
            //var d1 = lca.findDistance(x, 0);
            //var d2 = lca.findDistance(y, 0);
            return lca.findDistance(x, 0) + lca.findDistance(y, 0);
        }

        /// <summary>
        /// find the distance between root and node x,
        /// add d to the return value.
        /// if not found, return double.MinValue
        /// </summary>
        /// <param name="x"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        double findDistance(Tree<T> x, double d)
        {
            if (x == this)
            {
                return d;
            }
            foreach (var child in Children)
            {
                var td = child.findDistance(x, d + child.branchLength);
                if (td != double.MinValue)
                {
                    return td;
                }
            }
            return double.MinValue;
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
            if (tree.Name != "" && tree.Contents != null)
            {
                contents.Add(tree.Name, tree.Contents);
            }
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
