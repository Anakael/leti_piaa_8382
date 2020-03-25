using System;
using System.Collections.Generic;
using System.Linq;

namespace lab2
{
    /// <summary>
    /// Class for storing start and end points of graph
    /// </summary>
    public class Graph
    {
        /// <summary>
        /// Node in graph
        /// </summary>
        public class Node
        {
            /// <summary>
            /// Name of node
            /// </summary>
            public char Name { get; set; }

            /// <summary>
            /// Min distance to node
            /// </summary>
            public double? MinDistance { get; set; }

            /// <summary>
            /// Previous node in best way to node
            /// </summary>
            public Node CameFrom { get; set; }

            /// <summary>
            /// Children to children nodes
            /// </summary>
            public Dictionary<Node, double> Children { get; } = new Dictionary<Node, double>();
        }

        /// <summary>
        /// Start point
        /// </summary>
        public Node Start { get; set; }

        /// <summary>
        /// End point
        /// </summary>
        public List<Node> End { get; set; } = new List<Node>();

        /// <summary>
        /// Map of nodes in graph by their names
        /// </summary>
        public Dictionary<char, Node> Nodes { get; } = new Dictionary<char, Node>();
    }

    class Program
    {
        /// <summary>
        /// Read graph from stdin
        /// </summary>
        /// <returns>Graph</returns>
        static Graph ReadGraph()
        {
            var startEnd = Console.ReadLine().Split(' ');
            var graph = new Graph()
            {
                Start = new Graph.Node
                {
                    Name = startEnd[0].First()
                },
            };

            for (var i = 1; i < startEnd.Length; ++i)
            {
                graph.End.Add(new Graph.Node {Name = startEnd[i].First()});
            }

            while (true)
            {
                var input = Console.ReadLine()?.Split(' ');
                if (input == null || input.Length != 3)
                {
                    break;
                }

                var newNodeStart = new Graph.Node() {Name = input[0].First()};
                var newNodeEnd = new Graph.Node() {Name = input[1].First()};
                var distance = double.Parse(input[2]);

                Graph.Node existingStartNode = null;

                if (!graph.Nodes.TryGetValue(newNodeStart.Name, out existingStartNode))
                {
                    graph.Nodes.Add(newNodeStart.Name, newNodeStart);
                    existingStartNode = newNodeStart;
                }

                Graph.Node existingEndNode = null;
                if (!graph.Nodes.TryGetValue(newNodeEnd.Name, out existingEndNode))
                {
                    graph.Nodes.Add(newNodeEnd.Name, newNodeEnd);
                    existingEndNode = newNodeEnd;
                }

                existingStartNode.Children.Add(existingEndNode, distance);
            }

            graph.Start = graph.Nodes[graph.Start.Name];

            for (var i = 0; i < graph.End.Count; ++i)
            {
                graph.End[i] = graph.Nodes[graph.End[i].Name];
            }
            
            return graph;
        }

        /// <summary>
        /// Find greed way in graph
        /// </summary>
        /// <param name="graph">Graph for find.</param>
        /// <returns>Greed way to end node in graph as string</returns>
        static string FindGreedWay(Graph graph)
        {
            var visitedNodes = new Stack<Graph.Node>();
            visitedNodes.Push(graph.Start);

            while (visitedNodes.Count != 0)
            {
                var tmp = visitedNodes.First();
                while (tmp.Children.Count != 0)
                {
                    tmp = tmp.Children.OrderBy(x => x.Value).First().Key;
                    visitedNodes.Push(tmp);

                    if (graph.End.Contains(tmp))
                    {
                        return string.Join("", visitedNodes.Reverse().Select(x => x.Name));
                    }
                }

                visitedNodes.Pop();
                var last = visitedNodes.Peek();
                if (last.Children.Count != 0)
                {
                    last.Children.Remove(last.Children.OrderBy(x => x.Value).First().Key);    
                }
                
            }

            return string.Empty;
        }

        /// <summary>
        /// Find best way to end node in graph
        /// </summary>
        /// <param name="graph">Graph for find</param>
        /// <returns>Best way to end node in graph as string</returns>
        static string FindBestWay(Graph graph)
        {
            var nodesToVisit = new SortedDictionary<double, Queue<Graph.Node>> {{0, new Queue<Graph.Node>()}};
            nodesToVisit[0].Enqueue(graph.Start);
            nodesToVisit.First().Value.First().MinDistance = 0;

            while (nodesToVisit.Count != 0)
            {
                var tmp = nodesToVisit.First().Value.Dequeue();
                if (nodesToVisit.First().Value.Count == 0)
                {
                    nodesToVisit.Remove(nodesToVisit.First().Key);
                }

                if (graph.End.Contains(tmp))
                {
                    var bestWay = new Stack<Graph.Node>();
                    bestWay.Push(tmp);
                    while (tmp.CameFrom != null)
                    {
                        bestWay.Push(tmp.CameFrom);
                        tmp = tmp.CameFrom;
                    }


                    return string.Join("", bestWay.Select(x => x.Name));
                }

                foreach (var way in tmp.Children)
                {
                    var distance = tmp.MinDistance + way.Value ?? 0;
                    var heuristic = distance + graph.End.Min(x => x.Name) - way.Key.Name;

                    if (!nodesToVisit.ContainsKey(heuristic))
                    {
                        nodesToVisit[heuristic] = new Queue<Graph.Node>();
                    }


                    nodesToVisit[heuristic].Enqueue(way.Key);
                    if (way.Key.MinDistance == null || distance < way.Key.MinDistance)
                    {
                        way.Key.MinDistance = distance;
                        way.Key.CameFrom = tmp;
                    }
                }
            }

            return string.Empty;
        }

        static void Main(string[] args)
        {
            var graph = ReadGraph();
            Console.WriteLine(FindBestWay(graph)); // For greed way replace with FindGreedWay
        }
    }
}