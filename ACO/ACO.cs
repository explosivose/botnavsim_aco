//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

/* Ant Colony Optimization - Shortest Path Problem
 * Implementation by Matt Blickem
 * For use with BotNavSim https://github.com/explosivose/botnavsim
 * Requires reference to 
 * 		System		
 * 		System.Core		(for HashSet<T>)
 * 		Assembly-CSharp.dll (BotNavSim)
 * 		UnityEngine.dll (BotNavSim)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ACO {

	public class Arc {

		public Arc (Node n0, Node n1, float tau = 1f) {
			node0 = n0;
			node1 = n1;
			pheromone = tau;
		}
		
		public Node node0;
		public Node node1;
		public float pheromone;
		public float probability;

		public bool ConnectedTo(Node n) {
			if (node0 == n)
				return true;
			if (node1 == n)
				return true;
			return false;
		}

		public void DrawLine() {
			//Draw.Instance.Line(node0.position, node1.position, Color.white);
		}
	}

	public class Node {

		public Node(Vector3 location) {
			position = location;
		}

		public Vector3 position { get; private set; }
		public bool obstructed { get; set; }
	}

	public class SquareGraph {

		public SquareGraph(Vector3 min, Vector3 max, int nodeCount) {
			X = Y = nodeCount;
			spacing = (max.x - min.x) / (float)(nodeCount-1);
			spacing = Mathf.Max(spacing, (max.z - min.z)/(float)(nodeCount-1));
			nodes = new Node[X, Y];
			for (int x = 0; x < X; x++) {
				for (int y = 0; y < Y; y++) {
					Vector3 position = new Vector3(x * spacing, 0, y * spacing); 
					position += (min + Vector3.up*(max.y + min.y)/2f);
					Node n = new Node(position);
					nodes[x,y] = n;
				}
			}
			arcs = new HashSet<Arc>();
			ConnectNodes();
		}


		public float spacing { get; private set; }
		public int X {get; private set;}
		public int Y {get; private set;}
		private Node[,] nodes;
		private HashSet<Arc> arcs;

		// Connect nodes with set of arcs
		/* 
		 * Connections are as follows (tab == 4 spaces)
		 * Following this pattern and checking for
		 * graph boundaries will ensure no duplicate arcs
		 * and complete coverage.
		 * 					x+1,y-1
		 * 				___/	
		 * 			   /
		 * 			x,y-----x+1,y
		 * 			|  \___
		 * 			|	   \
		 * 			x,y+1	x+1,y+1
		 */ 
		/// <summary>
		/// <para>
		/// Connections are as follows (tab == 4 spaces)
		/// Following this pattern and checking for
		/// graph boundaries will ensure no duplicate arcs
		/// and complete coverage.</para>
		///	#########################<para></para>
		/// ###############(x+1,y-1)#<para></para>
		///	############___/#########<para></para>
		///	###########/#############<para></para>
		/// #######(x,y)-----(x+1,y)#<para></para>
		///	########|##\___##########<para></para>
		///	########|######\#########<para></para>
		///	######(x,y+1)##(x+1,y+1)#<para></para>
		/// #########################<para></para>
		/// </summary>
		void ConnectNodes() {
			for (int x = 0; x < X; x++) {
				for (int y = 0; y < Y; y++) {
					if (x < X-1) {
						arcs.Add(new Arc(
							nodes[x,y], nodes[x+1,y]));
						if (y > 0) arcs.Add(new Arc(
							nodes[x,y], nodes[x+1,y-1]));
					}
					if (y < Y-1) {
						arcs.Add(new Arc(
							nodes[x,y], nodes[x,y+1]));
						if (x < X-1) arcs.Add(new Arc(
							nodes[x,y], nodes[x+1,y+1]));
					}
				}
			}
		}


		/// <summary>
		/// Remove any arcs that connect node N
		/// </summary>
		/// <param name="n">Node to disconnect.</param>
		void DisconnectNode(Node n) {
			foreach (Arc a in arcs) {
				if (a.ConnectedTo(n)) 
					arcs.Remove(a);
			}
		}

		// Connect a single node to neighboring nodes.
		/// <summary>
		/// Connects a single node to unobstructed neighboring nodes.
		/// </summary>
		/// <param name="n">Node to connect.</param>
		void ConnectNode(Node n) {
			// PROBLEM: this method can create duplicate arcs!
			DisconnectNode(n);
			int x = 0, y = 0;
			bool nodeInSet = false;
			for (x = 0; x < X; x++) {
				for (y = 0; y < Y; y++) {
					if (nodes[x,y] == n) {
						nodeInSet = true;
						break;
					}
				}
			}
			if (nodeInSet) {
				if (y > 0) {
					arcs.Add(new Arc(
						nodes[x,y], nodes[x,y-1]));
					if (x > 0) arcs.Add(new Arc(
						nodes[x,y], nodes[x-1,y-1]));
					if (x < X-1) arcs.Add(new Arc(
						nodes[x,y], nodes[x+1,y-1]));
				}
				if (x > 0) {
					arcs.Add(new Arc(
						nodes[x,y], nodes[x-1,y]));
					if (y < Y-1) arcs.Add(new Arc(
						nodes[x,y], nodes[x-1,y+1]));
				}
				if (x < X-1) {
					arcs.Add(new Arc(
						nodes[x,y], nodes[x+1,y]));
					if (y < Y-1) arcs.Add(new Arc(
						nodes[x,y], nodes[x+1,y+1]));
				}
				if (y < Y-1) {
					arcs.Add(new Arc(
						nodes[x,y], nodes[x,y+1]));
				}
			} else {
				Debug.LogWarning("ACO: could not ConnectNode() because node not in graph.");
			}

			// Destroy any connections to obstructed nodes that we just created.
			DisconnectAllObstructedNodes();
		}

	
		/// <summary>
		/// Removes any arcs that connect obstructed nodes.
		/// </summary>
		public void DisconnectAllObstructedNodes() {
			foreach (Node n in nodes) {
				if (n.obstructed) DisconnectNode(n);
			}
		}



		public List<Arc> NodeArcs(Node n) {
			List<Arc> nodeArcs = new List<Arc>();
			foreach (Arc a in arcs) {
				if (a.ConnectedTo(n)) nodeArcs.Add(a);
			}
			return nodeArcs;
		}
	
		/// <summary>
		/// Returns the node whose position is nearest to a world coordinate.
		/// </summary>
		/// <returns>The node with position nearest to world coordinate.</returns>
		/// <param name="worldPosition">World position.</param>
		public Node NearestNode(Vector3 worldPosition) {
			Node nearestNode = nodes [0, 0];
			float d1 = Mathf.Infinity;
			foreach (Node n in nodes) {
				float d2 = Vector3.Distance(n.position, worldPosition);
				if (d2 < d1) {
					nearestNode = n;
					d1 = d2;
				}
			}
			return nearestNode;
		}

		public void DrawDebug() {
			foreach (Arc a in arcs)
				a.DrawLine();
		}
	}

	public class Ant {

		public enum Mode { forward, backward }

		public Ant(SquareGraph graph, Node origin, Node destination) {
			myNode = origin;
			nest = origin;
			myPath = new List<Arc>();
			food = destination;
			g = graph;
			mode = Mode.forward;
		}

		public Node myNode {get; private set;}
		public Mode mode {
			get { return _mode; }
			set {
				_mode = value;
				if (_mode == Mode.forward) {
					debugColor = Color.black;
					debugColor.a = 0.01f;
				} else if (_mode == Mode.backward) {
					debugColor = Color.white;
					debugColor.a = 0.1f;
					tau = Mathf.Max(g.X, g.Y)/(float)myPath.Count;
				} else {
					Debug.LogError("ACO: Ant Mode invalid!");
					mode = Mode.forward;
				}
			}
		}

		private Mode _mode;
		private Node food;
		private Node nest;
		private Color debugColor;
		private SquareGraph g;
		private List<Arc> myArcs;
		private List<Arc> myPath;
		private float tau;

		public void DrawDebug() {
			if (myPath.Count < 1) return;
			for(int i = 0; i < myPath.Count; i++) {
				Draw.Instance.Line(
					myPath[i].node0.position,
					myPath[i].node1.position, 
					debugColor);
			}
			Vector3 myPosition = myPath[myPath.Count-1].node0.position;
			myPosition += myPath[myPath.Count-1].node1.position;
			myPosition /= 2f;
			Draw.Instance.Cube(
				myPosition,
				Vector3.one,
				debugColor);
		}

		public void Update() {
			if (mode == Mode.forward) {
				ForwardUpdate();
			}
			else if (mode == Mode.backward) {
				BackwardUpdate();
			}
		}

		// in forward mode ants wander around until they find the destination
		// using pheromone to make decisions about which arc to take
		private void ForwardUpdate() {
			// collate local arcs
			myArcs = g.NodeArcs(myNode);

			// if my node isn't connected by any arcs I'm stuck!
			if (myArcs.Count < 1) {
				Debug.LogError("ACO: Ant on disconnected Node");
				return;
			}
			else {
				// remove the arc that leads to the previous node in my path
				if (myPath.Count > 0)
					myArcs.Remove(myPath[myPath.Count-1]);
			}

			// calculate probabilities for each arc
			float phSum = 0f;
			foreach(Arc a in myArcs) 
				phSum += a.pheromone;
	
			foreach(Arc a in myArcs)
				a.probability = (a.pheromone)/(phSum);

			// sort by probability in ascending order
			myArcs.Sort(delegate(Arc x, Arc y) {
				return x.probability.CompareTo(y.probability);
			});

			// select arc based on probability
			float cumulative = 0f;
			float roll = UnityEngine.Random.value;
			Arc selection = myArcs[0];
			foreach(Arc a in myArcs) {
				if (roll > cumulative && roll < cumulative + a.probability) {
					selection = a;
					break;
				}
				cumulative += a.probability;
			}

			// move to node
			if (myNode == selection.node0) {
				myNode = selection.node1;
			}
			else if (myNode == selection.node1) {
				myNode = selection.node0;
			}
			else {
				Debug.LogError("ACO: Ant in forward mode somehow selected arc not connected to myNode");
			}

			myPath.Add(selection);

			// if we found the food 
			if (myNode == food) {
				EliminateLoopsInMyPath();
				mode = Mode.backward;
			}
		}

		// in backward mode ants follow their own path back to the origin
		// dropping pheromone
		private void BackwardUpdate() {
			// move to next node in path
			Arc selection = myPath[myPath.Count-1];
			/*if (myNode == selection.node0) {
				myNode = selection.node1;
			}
			else if (myNode == selection.node1) {
				myNode = selection.node0;
			}
			else {
				Debug.LogError("ACO: Ant in backward mode somehow selected arc not connected to myNode");
			}*/
			selection.pheromone += tau;
			myPath.Remove(selection);
			if (myPath.Count < 1) {
				myNode = nest;
				mode = Mode.forward;
			}
		}

		private void EliminateLoopsInMyPath(int scanIndex = 0) {
			for(int i = myPath.Count-1; i > 0; i--) {
				if (myPath[i] == myPath[scanIndex]) {
					if (i != scanIndex) {
						myPath.RemoveRange(scanIndex, i - scanIndex);
						i = scanIndex;
					}
				}
			}
			if (++scanIndex < myPath.Count-1)
				EliminateLoopsInMyPath(scanIndex);
		}
	}

	public class ACO : INavigation {

		public ACO() {

		}

		private Bounds bounds;
		private SquareGraph graph;
		private List<Ant> ants;

		public Bounds searchBounds {
			get {
				return bounds;
			}
			set {
				bounds = value;
				graph = new SquareGraph(bounds.min, bounds.max, 50);
			}
		}
		
		public Vector3 origin {get;set;}
		
		public Vector3 destination {get;set;}
		
		public bool pathFound {
			get {
				return false;
			}
		}
		
		public Space spaceRelativeTo {
			get {
				return Space.World;
			}
		}

		public IEnumerator SearchForPath() {
			ants = new List<Ant>();
			Node nest = graph.NearestNode(origin);
			Node food = graph.NearestNode(destination);
			for (int i = 0; i < 100; i++)
				ants.Add(new Ant(graph, nest, food));

			for(int i = 0; i < 1000; i++) {
				foreach(Ant ant in ants) {
					ant.Update();

				}
				yield return new WaitForEndOfFrame();
			}

			Debug.Log("ACO: Search Complete.");
		}

		public IEnumerator SearchForPath (Vector3 start, Vector3 end) {
			origin = start;
			destination = end;
			yield return Simulation.Instance.StartCoroutine(SearchForPath());
		}

		public Vector3 PathDirection (Vector3 myLocation) {
			return Vector3.zero;
		}

		public void Proximity (Vector3 from, Vector3 to, bool obstructed) {

		}

		public void DrawGizmos () {

		}

		public void DrawDebugInfo () {
			graph.DrawDebug();
			foreach(Ant ant in ants) {
				ant.DrawDebug();
			}
		}


	}
}

