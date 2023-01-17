using UnityEngine;
using System.Collections;
using System.Collections.Generic;

///Calculates paths using A*
static class AStar {

	private static float heuristicWeight = 1;
	private static float yieldInterval = 0.1f;

	//custom list for A* algorith
	public class PriorityQueue : IComparer{
		
		private List<PathNode> nodes = new List<PathNode>();
			
		public int Compare ( System.Object a ,   System.Object b  ){ 
	        PathNode nodeA = a as PathNode;
	        PathNode nodeB = b as PathNode;
	        if ( nodeA.estimatedCost < nodeB.estimatedCost){
				return -1;
			} else if (nodeA.estimatedCost > nodeB.estimatedCost){
				return 1;
			} else {
				return 0;
	    	}
	    }

	    public int Count{
	    	get {return nodes.Count;}
	    }

		public int Push ( PathNode node  ){
			nodes.Add(node);
			nodes.Sort();
			return nodes.Count;
		}
		
		public PathNode Front (){
			if (nodes.Count > 0){
				return nodes[0];
			} else {
				return null;
			}
		}
		
		public bool Contains ( PathNode node  ){
			 return nodes.Contains(node);
		}
		
		public void  Remove ( PathNode node  ){
			nodes.Remove(node);	
			nodes.Sort();
		}
	}

	//A* implementation
	public static IEnumerator CalculatePath(PathNode start, PathNode end, List<PathNode> allNodes, System.Action<List<Vector2>> callback){

		float timer = 0;

		PriorityQueue openList= new PriorityQueue();
		PriorityQueue closedList= new PriorityQueue();

		openList.Push(start);
		start.cost = 0;
		start.estimatedCost = HeuristicEstimate(start, end, heuristicWeight);

		PathNode currentNode = null;

		while(openList.Count != 0){

			currentNode = openList.Front();
			if (currentNode == end)
				break;

			List<int> links = currentNode.links;

			for (int i= 0; i != links.Count; i++){

				PathNode endNode = allNodes[links[i]];

				float incrementalCost = GetCost(currentNode, endNode);
				float endNodeCost = currentNode.cost + incrementalCost;

				if (closedList.Contains(endNode)){
					if (endNode.cost <= endNodeCost)
						continue;

					closedList.Remove(endNode);

				} else if (openList.Contains(endNode)){
					if (endNode.cost <= endNodeCost)
						continue;
				}

				float endNodeHeuristic = HeuristicEstimate(endNode, end, heuristicWeight);
				endNode.cost = endNodeCost;
				endNode.parent = currentNode;
				endNode.estimatedCost = endNodeCost + endNodeHeuristic;

				if (!openList.Contains(endNode))
					openList.Push(endNode);
			}

			closedList.Push(currentNode);
			openList.Remove(currentNode);

			timer = Mathf.Repeat(timer + Time.deltaTime, yieldInterval + 1);
			if (timer >= yieldInterval)
				yield return null;
		}

		if (!currentNode.Equals(end)){
			
			// Debug.LogWarning("No path found :(");
			callback(new List<Vector2>());
		
		} else {

			List<Vector2> path= new List<Vector2>();

			while (currentNode != null){

				path.Add(currentNode.pos);
				currentNode = currentNode.parent;
			}

			path.Reverse();
			callback(path);
		}
	}


	private static float HeuristicEstimate ( PathNode currentNode, PathNode endNode, float heuristicWeight ){
		return (currentNode.pos - endNode.pos).magnitude * heuristicWeight;
	}
	
	private static float GetCost ( PathNode nodeA, PathNode nodeB){
		return (nodeA.pos - nodeB.pos).magnitude;
	}
}