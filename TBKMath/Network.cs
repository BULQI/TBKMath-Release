using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBKMath
{
    /// <summary>
    /// A graph in which nodes are of native type T, and links are abstract relationships.
    /// </summary>
    /// <typeparam name="T">The native type of the nodes.</typeparam>
    public class Network<T>
    {
        /// <summary>
        /// The nodes of the graph.
        /// </summary>
        public List<T> Nodes;
        /// <summary>
        /// Each node points to a list of nodes: its neighbors.
        /// </summary>
        public Dictionary<T, List<T>> Neighbors;
        /// <summary>
        /// Indicates whether the graph is directed or (Directed=true) or not.
        /// </summary>
        public bool Directed;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="directed">Determines whether the graph is directed or not (the default).</param>
        public Network(bool directed = false)
        {
            Nodes = new List<T>();
            Neighbors = new Dictionary<T, List<T>>();
            Directed = directed;
        }

        /// <summary>
        /// Attaches a node and makes the requested links.
        /// </summary>
        /// <param name="node">The node to be attached.</param>
        /// <param name="neighbors">The nodes--old or new--to which node is to have links.</param>
        public void AttachNode(T node, List<T> neighbors)
        {
            // If node is new, add it to Nodes and add a new list to Neighbors
            if (!Nodes.Contains(node))
            {
                Nodes.Add(node);
                Neighbors.Add(node, neighbors);
            }
            else
            {
                // If node is already in Nodes, add the non-attached neighbors to node's neighbor list.
                foreach (T neighbor in neighbors)
                {
                    if (!Neighbors[node].Contains(neighbor))
                    {
                        Neighbors[node].Add(neighbor);
                    }
                }
            }

            // Ensure that all of the node's neighbors are in Nodes
            foreach (T neighbor in neighbors)
            {
                if (!Nodes.Contains(neighbor))
                {
                    Nodes.Add(neighbor);
                    Neighbors.Add(neighbor, new List<T>());
                }
                // if the graph is not directed, add node to the Neighbor list for each neighbor.
                if (!Neighbors[neighbor].Contains(node) & !Directed)
                {
                    Neighbors[neighbor].Add(node);
                }
            }
        }

        /// <summary>
        /// Removes a node from the network and makes the other required adjustments to the structure.
        /// </summary>
        /// <param name="node">The node to be removed.</param>
        public void DetachNode(T node)
        {
            if (!Nodes.Contains(node))
                return;

            // loop over neighbors in node's neigbors
            foreach (T neighbor in Neighbors[node])
            {
                // if the graph is directed, remove links from each neighbor to node
                if (Directed)
                {
                    Neighbors[neighbor].Remove(node);
                }
            }

            // remove the node from Nodes and its links from Neighbors
            Nodes.Remove(node);
            Neighbors.Remove(node);
        }

        ///// <summary>
        ///// This method has not been kept up-to date. Please use the other version.
        ///// </summary>
        ///// <param name="network"></param>
        ///// <returns></returns>
        //public static List<List<T>> GetConnectedComponentsList(Network<T> network)
        //{
        //    List<List<T>> connectedComponents = new List<List<T>>();
        //    Dictionary<T, bool> visited = new Dictionary<T, bool>();
        //    foreach (T node in network.Nodes)
        //    {
        //        visited.Add(node, false);
        //    }

        //    int compNum = 0;
        //    foreach (T node in network.Nodes)
        //    {
        //        if (!visited[node])
        //        {
        //            compNum++;
        //            Queue<T> q = new Queue<T>();
        //            q.Enqueue(node);
        //            List<T> connectedComponent = new List<T>();
        //            connectedComponent.Add(node);
        //            visited[node] = true;
        //            while (q.Count > 0)
        //            {
        //                T w = q.Dequeue();
        //                foreach (T neighbor in network.Neighbors[w])
        //                {
        //                    if (!visited[neighbor])
        //                    {
        //                        visited[neighbor] = true;
        //                        q.Enqueue(neighbor);
        //                        connectedComponent.Add(neighbor);
        //                    }
        //                }
        //            }
        //            connectedComponents.Add(connectedComponent);
        //        }
        //    }

        //    return connectedComponents;
        //}

        /// <summary>
        /// Takes a network and returns a list of disjoint networks, each of which is connected, and the union of which is the network.
        /// WARNING: this method does not yet work properly for directed networks
        /// </summary>
        /// <param name="network">The network to decompose.</param>
        /// <returns>A list comprising the connected components of argument network.</returns>
        public static List<Network<T>> GetConnectedComponents(Network<T> network)
        {
            List<Network<T>> connectedComponents = new List<Network<T>>();
            Dictionary<T, bool> visited = new Dictionary<T, bool>();
            foreach (T node in network.Nodes)
            {
                visited.Add(node, false);
            }

            int compNum = 0;
            // loop over all nodes in the main network
            // in each pass, identify a node that has not been visited to now, and
            // find the connected component to which it belongs, visiting each node along the way.
            foreach (T node in network.Nodes)
            {
                // if the node has not been visited yet, form a new connected component.
                if (!visited[node])
                {
                    compNum++;
                    Queue<T> q = new Queue<T>(); // FIFO
                    // form a new queue and enqueue the starting node
                    q.Enqueue(node);
                    Network<T> connectedComponent = new Network<T>();
                    // form a new network and attach the starting node and all of its neighbors
                    connectedComponent.AttachNode(node, network.Neighbors[node]);
                    visited[node] = true;
                    while (q.Count > 0)
                    {
                        // pop the oldest node (FIFO) off the queue, call it w
                        T w = q.Dequeue();
                        // loop over all nodes in the neighborhood of w
                        foreach (T neighbor in network.Neighbors[w])
                        {
                            // if neighbor has not been visited before, mark it "visited"
                            // add it to the queue
                            // and attach it to the current connected component with all of its neighbors
                            if (!visited[neighbor])
                            {
                                visited[neighbor] = true;
                                q.Enqueue(neighbor);
                                connectedComponent.AttachNode(w, network.Neighbors[w]);
                            }
                        }
                    }
                    connectedComponents.Add(connectedComponent);
                }
            }

            return connectedComponents;
        }

        /// <summary>
        /// Adds a new link between existing nodes. Enforces directedness type.
        /// </summary>
        /// <param name="nodefrom">The node from which the link arises.</param>
        /// <param name="nodeto">The node to which the link points.</param>
        /// <returns>Returns true if the operation succeeded.</returns>
        public bool AddLink(T nodefrom, T nodeto)
        {
            if (!Nodes.Contains(nodefrom) || !Nodes.Contains(nodeto))
                return false;

            if (!Neighbors[nodefrom].Contains(nodeto))
            {
                Neighbors[nodefrom].Add(nodeto);
            }

            if (!Directed & !Neighbors[nodeto].Contains(nodefrom))
            {
                Neighbors[nodeto].Add(nodeto);
            }

            return true;
        }

        /// <summary>
        /// Removes a link from between two existing nodes. Enforces directedness type.
        /// </summary>
        /// <param name="nodefrom">The node from which the link arises.</param>
        /// <param name="nodeto">The node to which the link points.</param>
        /// <returns></returns>
        public bool RemoveLink(T nodefrom, T nodeto)
        {
            if (!Nodes.Contains(nodefrom) || !Nodes.Contains(nodeto))
                return false;

            if (Neighbors[nodefrom].Contains(nodeto))
            {
                Neighbors[nodefrom].Remove(nodeto);
            }

            if (!Directed & Neighbors[nodeto].Contains(nodefrom))
            {
                Neighbors[nodeto].Remove(nodeto);
            }

            return true;
        }
    }
}
