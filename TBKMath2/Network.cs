using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBKMath
{
    public class Network<T>
    {
        public List<T> Nodes;
        public Dictionary<T, List<T>> Neighbors;

        public Network()
        {
            Nodes = new List<T>();
            Neighbors = new Dictionary<T, List<T>>();
        }

        public void AttachNode(T node, List<T> neighbors)
        {
            if (!Nodes.Contains(node))
            {
                Nodes.Add(node);
                Neighbors.Add(node, neighbors);
            }
            else
            {
                foreach (T neighbor in neighbors)
                {
                    if (!Neighbors[node].Contains(neighbor))
                    {
                        Neighbors[node].Add(neighbor);
                    }
                }
            }

            foreach (T neighbor in neighbors)
            {
                if (!Nodes.Contains(neighbor))
                {
                    Nodes.Add(neighbor);
                    Neighbors.Add(neighbor, new List<T>());
                }
                if (!Neighbors[neighbor].Contains(node))
                {
                    Neighbors[neighbor].Add(node);
                }
            }
        }

        public void DetachNode(T node)
        {
            if (!Nodes.Contains(node))
                return;

            Nodes.Remove(node);
            foreach (T neighbor in Neighbors[node])
            {
                Neighbors[neighbor].Remove(node);
            }

            Neighbors.Remove(node);
        }

        public static List<List<T>> GetConnectedComponentsList(Network<T> network)
        {
            List<List<T>> connectedComponents = new List<List<T>>();
            Dictionary<T, bool> visited = new Dictionary<T, bool>();
            foreach (T node in network.Nodes)
            {
                visited.Add(node, false);
            }

            int compNum = 0;
            foreach (T node in network.Nodes)
            {
                if (!visited[node])
                {
                    compNum++;
                    Queue<T> q = new Queue<T>();
                    q.Enqueue(node);
                    List<T> connectedComponent = new List<T>();
                    connectedComponent.Add(node);
                    visited[node] = true;
                    while (q.Count > 0)
                    {
                        T w = q.Dequeue();
                        foreach (T neighbor in network.Neighbors[w])
                        {
                            if (!visited[neighbor])
                            {
                                visited[neighbor] = true;
                                q.Enqueue(neighbor);
                                connectedComponent.Add(neighbor);
                            }
                        }
                    }
                    connectedComponents.Add(connectedComponent);
                }
            }

            return connectedComponents;
        }

        public static List<Network<T>> GetConnectedComponents(Network<T> network)
        {
            List<Network<T>> connectedComponents = new List<Network<T>>();
            Dictionary<T, bool> visited = new Dictionary<T, bool>();
            foreach (T node in network.Nodes)
            {
                visited.Add(node, false);
            }

            int compNum = 0;
            foreach (T node in network.Nodes)
            {
                if (!visited[node])
                {
                    compNum++;
                    Queue<T> q = new Queue<T>();
                    q.Enqueue(node);
                    Network<T> connectedComponent = new Network<T>();
                    connectedComponent.AttachNode(node, network.Neighbors[node]);
                    visited[node] = true;
                    while (q.Count > 0)
                    {
                        T w = q.Dequeue();
                        foreach (T neighbor in network.Neighbors[w])
                        {
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

    }
}
