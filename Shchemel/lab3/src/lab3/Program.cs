using System;
using System.Collections.Generic;
using System.Linq;

namespace lab3
{
	/// <summary>
	/// Class wrapper for printing
	/// </summary>
	public static class Logger
	{
		public enum LogLevel
		{
			Debug,
			Info
		}

		/// <summary>
		/// Log message with specify log level
		/// </summary>
		/// <param name="text"></param>
		/// <param name="level"></param>
		public static void Log(object text, LogLevel level = LogLevel.Info)
		{
			int logLevel;
			var hasVariable = int.TryParse(Environment.GetEnvironmentVariable("LAB3_LOG_LEVEL"), out logLevel);
			logLevel = hasVariable ? logLevel : (int)LogLevel.Info;

			if ((int)level >= logLevel)
			{
				Console.WriteLine(text.ToString());
			}
		}
	}

	/// <summary>
	/// Extensions for <see cref="Queue{T}"/>
	/// </summary>
	public static class QueueExtension
	{
		/// <summary>
		/// Add element to Queue and return Queue
		/// </summary>
		/// <param name="queue"><see cref="Queue{T}"/></param>
		/// <param name="elem">First element to add</param>
		/// <typeparam name="T">Type in Queue</typeparam>
		/// <returns>New queue</returns>
		public static Queue<T> Add<T>(this Queue<T> queue, T elem)
		{
			queue.Enqueue(elem);
			return queue;
		}
	}

	/// <summary>
	/// Class for storing start and end points of graph
	/// </summary>
	public class Graph
	{
		/// <summary>
		/// Map of nodes in graph by their names
		/// </summary>
		private readonly Dictionary<char, Node> _nodes = new Dictionary<char, Node>();

		private readonly List<Edge> _edges = new List<Edge>();

		/// <summary>
		/// Print all edges in graph as {From} {To} {Flow}
		/// </summary>
		public void PrintEdges()
		{
			_edges.OrderBy(x => x.From.Name)
				.ThenBy(x => x.To.Name)
				.ToList()
				.ForEach(x => Logger.Log($"{x.From.Name} {x.To.Name} {x.Flow.Current}"));
		}

		/// <summary>
		/// Read graph from stdin
		/// </summary>
		/// <returns>Graph</returns>
		public void ReadGraph()
		{
			var countEdges = int.Parse(Console.ReadLine()?.Split(' ').First());
			var start = Console.ReadLine().Split(' ').First().First();
			var end = Console.ReadLine().Split(' ').First().First();

			var sourceName = start;
			var sinkName = end;
			Source = new Node { Name = sourceName };
			Sink = new Node { Name = sinkName };
			_nodes.Add(sourceName, Source);
			_nodes.Add(sinkName, Sink);

			for (var i = 0; i < countEdges; i++)
			{
				var input = Console.ReadLine()?.Split(' ');

				var newNodeStart = new Node() { Name = (input[0]).First() };
				var newNodeEnd = new Node() { Name = input[1].First() };
				var maxFlowForNode = int.Parse(input[2]);

				Node fromNode = null;

				if (!_nodes.TryGetValue(newNodeStart.Name, out fromNode))
				{
					_nodes.Add(newNodeStart.Name, newNodeStart);
					fromNode = newNodeStart;
				}

				Node toNode = null;
				if (!_nodes.TryGetValue(newNodeEnd.Name, out toNode))
				{
					_nodes.Add(newNodeEnd.Name, newNodeEnd);
					toNode = newNodeEnd;
				}

				/// Add just one node for child and parent
				var edge = new Edge { From = fromNode, To = toNode, Flow = new Edge.FlowEntity { Max = maxFlowForNode } };
				fromNode.Children.Add(edge);
				toNode.Parents.Add(edge);
				_edges.Add(edge);
			}
		}

		/// <summary>
		/// Find max flow in stream
		/// </summary>
		/// <returns>Value of flow</returns>
		public int FindMaxFlow()
		{
			var nodesToVisit = new Queue<Node>().Add(Source);
			var visitedNodes = new HashSet<Node>();

			while (nodesToVisit.Count != 0)
			{
				var tmp = nodesToVisit.First();

				Logger.Log($"Current node => {tmp.Name}", Logger.LogLevel.Debug);

				if (tmp == Sink)
				{
					Logger.Log("Start patch", Logger.LogLevel.Debug);
					PatchFlowInPath(); // Change glow in path
					nodesToVisit = new Queue<Node>().Add(Source);
					visitedNodes.Clear();
					continue;
				}

				/// Search path to sink
				foreach (var edge in tmp.Children
					.OrderBy(x => x.To.Name)
					.Where(x => !x.IsFullFlow) // In direct direction
					.Concat(tmp.Parents
						.OrderBy(x => x.From.Name)
						.Where(x => !x.IsEmptyFlow)) // In reverse direction
					)
				{
					var from = edge.From;
					var to = edge.To;
					if (tmp == from && !visitedNodes.Contains(to)) // Came from direct direction
					{
						to.CameFrom = edge;
						nodesToVisit.Enqueue(to);
					}
					else if (tmp == edge.To && !visitedNodes.Contains(from)) // Came from riverse direction
					{
						from.CameFrom = edge;
						nodesToVisit.Enqueue(from);
					}
				}

				visitedNodes.Add(tmp);
				nodesToVisit.Dequeue();
			}

			return Sink.Parents.Sum(x => x.Flow.Current);
		}

		/// <summary>
		/// Find max available flow and fill path with it
		/// </summary>
		private void PatchFlowInPath()
		{
			var tmpNode = Sink;
			var tmpEdge = Sink.CameFrom;
			var minFlow = tmpEdge.Flow.Max;

			while (tmpNode != Source)
			{
				var isDirectWay = tmpEdge.To == tmpNode;
				var tmpFlow = isDirectWay ? tmpEdge.Flow.Max - tmpEdge.Flow.Current : tmpEdge.Flow.Current;
				if (tmpFlow < minFlow)
				{
					minFlow = tmpFlow;
				}

				tmpNode = isDirectWay ? tmpEdge.From : tmpEdge.To;
				tmpEdge = tmpNode.CameFrom;
			}

			Logger.Log($"Min flow = {minFlow}", Logger.LogLevel.Debug);

			tmpNode = Sink;
			tmpEdge = Sink.CameFrom;

			while (tmpNode != Source)
			{
				var isDirectWay = tmpEdge.To == tmpNode;
				if (isDirectWay)
				{
					tmpEdge.Flow.Current += minFlow;
				}
				else
				{
					tmpEdge.Flow.Current -= minFlow;
				}

				Logger.Log($"Flow from {tmpEdge.From.Name} to {tmpEdge.To.Name} = {tmpEdge.Flow.Current} (max = {tmpEdge.Flow.Max})", Logger.LogLevel.Debug);

				if (tmpEdge.IsFullFlow)
				{
					Logger.Log($"Edge from {tmpEdge.From.Name} to {tmpEdge.To.Name} is full now", Logger.LogLevel.Debug);
				}

				tmpNode = isDirectWay ? tmpEdge.From : tmpEdge.To;
				tmpEdge = tmpNode.CameFrom;
			}
		}

		/// <summary>
		/// Edge between two nodes
		/// </summary>
		private class Edge
		{
			public class FlowEntity
			{
				/// <summary>
				/// Max flow
				/// </summary>
				public int Max { get; set; }

				/// <summary>
				/// Current flow 
				/// </summary>
				public int Current { get; set; }
			}

			/// <summary>
			/// <see cref="Flow"/>
			/// </summary>
			public FlowEntity Flow { get; set; }

			/// <summary>
			/// Start <see cref="Node"/>
			/// </summary>
			public Node From { get; set; }

			/// <summary>
			/// Target <see cref="Node"/>
			/// </summary>
			public Node To { get; set; }

			/// <summary>
			/// Check that <see cref="FlowEntity"/> has max value
			/// </summary>
			public bool IsFullFlow
			{
				get { return Flow.Current == Flow.Max; }
			}

			/// <summary>
			/// Check that <see cref="FlowEntity"/> has zero value
			/// </summary>
			public bool IsEmptyFlow
			{
				get { return Flow.Current == 0; }
			}
		}

		/// <summary>
		/// Node in graph
		/// </summary>
		private class Node
		{
			/// <summary>
			/// Name of node
			/// </summary>
			public char Name { get; set; }

			/// <summary>
			/// Children to children nodes
			/// </summary>
			public List<Edge> Children { get; } = new List<Edge>();

			/// <summary>
			/// Parents of children nodes
			/// </summary>
			public List<Edge> Parents { get; } = new List<Edge>();

			/// <summary>
			/// <see cref="Node"/> parent node in path
			/// </summary>
			public Edge CameFrom { get; set; }

		}

		/// <summary>
		/// Start point
		/// </summary>
		private Node Source { get; set; }

		/// <summary>
		/// End point
		/// </summary>
		private Node Sink { get; set; }
	}

	class Program
	{
		static void Main(string[] args)
		{
			var graph = new Graph();
			graph.ReadGraph();
			Logger.Log(graph.FindMaxFlow());
			graph.PrintEdges();
		}
	}
}