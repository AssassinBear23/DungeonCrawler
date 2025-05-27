using System.Collections.Generic;
using UnityEngine;

namespace Player.Pathfinding
{
    using Dungeon.Data;
    using Dungeon.DataStructures;
    using Dungeon.Utilities;
    using Movement;
    using System.Linq;
    using UnityEditor;

    /// <summary>
    /// Handles pathfinding logic for the player using an A* algorithm on a generated graph.
    /// </summary>
    public class PathFinder : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PathfindingGraphGenerator graphGenerator;

        [Header("Debug Variables")]
        [SerializeField] private List<Vector3> path = new();
        [SerializeField] private SerializedDictionary<Vector3, float> costDictionary = new();
        private Graph<Vector3> graph;
        [SerializeField] private bool showDiscovered;
        [SerializeField] private bool showPath;
        [SerializeField] private bool showCost;

        [Space(10)]
        [SerializeField] private Vector3 startNode;
        [SerializeField] private Vector3 endNode;

        /// <summary>
        /// Set of nodes discovered during pathfinding, used for debugging and visualization.
        /// </summary>
        private HashSet<Vector3> discovered = new();



        /// <summary>
        /// Initializes the pathfinder by retrieving the graph generator and its graph.
        /// </summary>
        void Start()
        {
            //graphGenerator = GetComponent<PathfindingGraphGenerator>();
            graph = graphGenerator.Graph;
        }

        public void SetPlayerPosition()
        {
            transform.position = AlgorithmsUtils.CalculateMiddlePosition(DungeonDataGenerator.Instance.StarterRoom);
        }

        /// <summary>
        /// Finds the closest node in the graph to the specified position.
        /// </summary>
        /// <param name="position">The position to find the closest node to.</param>
        /// <returns>The closest node in the graph to the given position.</returns>
        private Vector3 GetClosestNodeToPosition(Vector3 position)
        {
            Vector3 closestNode = Vector3.zero;
            float closestDistance = Mathf.Infinity;

            List<Vector3> nodes = graph.GetNodes();

            foreach (Vector3 node in nodes)
            {
                float currentNodeDistance = Vector2.Distance(new Vector2(position.x, position.z), new Vector2(node.x, node.z));
                if (currentNodeDistance < closestDistance)
                {
                    closestNode = node;
                    closestDistance = currentNodeDistance;
                }
            }
            Debug.Log($"\nclosestNode = {closestNode}\nDistance = {closestDistance}");

            return closestNode;
        }

        /// <summary>
        /// Calculates the shortest path between two positions using the A* algorithm.
        /// </summary>
        /// <param name="from">The starting position.</param>
        /// <param name="to">The target position.</param>
        /// <returns>A list of Vector3 positions representing the shortest path, or an empty list if no path is found.</returns>
        public List<Vector3> CalculatePath(Vector3 from, Vector3 to, PathFindingType toUsePathFinding)
        {
            Vector3 playerPosition = from;

            startNode = GetClosestNodeToPosition(playerPosition);
            endNode = GetClosestNodeToPosition(to);

            List<Vector3> shortestPath = new();

            if (toUsePathFinding == PathFindingType.AStar)
                shortestPath = AStar(startNode, endNode);
            else if (toUsePathFinding == PathFindingType.Recursion)
                shortestPath = RecursiveDFS(startNode, endNode, new());

            path = shortestPath; //Used for drawing the path

            return shortestPath;
        }

        /// <summary>
        /// Performs the A* pathfinding algorithm to find the shortest path between two nodes.
        /// </summary>
        /// <param name="start">The starting node.</param>
        /// <param name="end">The target node.</param>
        /// <returns>A list of Vector3 positions representing the shortest path, or an empty list if no path is found.</returns>
        private List<Vector3> AStar(Vector3 start, Vector3 end)
        {
            discovered.Clear();
            costDictionary.Clear();

            Graph<Vector3> graph = graphGenerator.Graph;
            PriorityQueue<Vector3> priorityQueue = new();
            Dictionary<Vector3, Vector3> pathDictionary = new();

            Vector3 current = start;
            priorityQueue.Enqueue(current, 0);
            costDictionary[current] = 0;

            while (priorityQueue.Count > 0)
            {
                current = priorityQueue.Dequeue();
                discovered.Add(current);
                if (current == end)
                {
                    return ReconstructPath(pathDictionary, start, end);
                }
                foreach (Vector3 neighbour in graph.GetNeighbours(current))
                {
                    float newCost = costDictionary[current] + Cost(current, neighbour);
                    if (!costDictionary.ContainsKey(neighbour) || newCost < costDictionary[neighbour])
                    {
                        costDictionary[neighbour] = newCost;
                        pathDictionary[neighbour] = current;
                        priorityQueue.Enqueue(neighbour, newCost + Heuristic(neighbour, end));
                    }
                }
            }
            return new List<Vector3>(); // No path found
        }

        /// <summary>
        /// Performs a recursive depth-first search (DFS) to find a path from the current node to the end node.
        /// </summary>
        /// <param name="current">The node currently being explored.</param>
        /// <param name="end">The target node to reach.</param>
        /// <param name="visited">A list of nodes that have already been visited to prevent cycles.</param>
        /// <returns>
        /// A list of <see cref="Vector3"/> positions representing the path from the current node to the end node,
        /// or <c>null</c> if no path is found.
        /// </returns>
        private List<Vector3> RecursiveDFS(Vector3 current, Vector3 end, List<Vector3> visited)
        {
            if (current == end) return new() { end };

            visited.Add(current);
            discovered.Add(current);

            List<Vector3> neighbours = graph.GetNeighbours(current);

            neighbours.Sort((a, b) => Heuristic(a, end).CompareTo(Heuristic(b,end))); // Sort by distance to end node

            foreach (var neighbor in neighbours)
            {
                if (visited.Contains(neighbor)) continue;

                var path = RecursiveDFS(neighbor, end, visited);
                
                if (path != null && path.Count > 0)
                {
                    path.Insert(0, current);
                    return path;
                }
            }
            return null;
        }

        /// <summary>
        /// Calculates the movement cost between two nodes.
        /// </summary>
        /// <param name="from">The starting node.</param>
        /// <param name="to">The target node.</param>
        /// <returns>The cost of moving from the starting node to the target node.</returns>
        public float Cost(Vector3 from, Vector3 to)
        {
            return Vector3.Distance(from, to);
        }

        /// <summary>
        /// Estimates the cost from one node to another (heuristic for A*).
        /// </summary>
        /// <param name="from">The starting node.</param>
        /// <param name="to">The target node.</param>
        /// <returns>The estimated cost from the starting node to the target node.</returns>
        public float Heuristic(Vector3 from, Vector3 to)
        {
            return Vector3.Distance(from, to);
        }

        /// <summary>
        /// Reconstructs the path from the start node to the end node using the parent map.
        /// </summary>
        /// <param name="parentMap">A dictionary mapping each node to its parent in the path.</param>
        /// <param name="start">The starting node.</param>
        /// <param name="end">The target node.</param>
        /// <returns>A list of Vector3 positions representing the reconstructed path.</returns>
        private List<Vector3> ReconstructPath(Dictionary<Vector3, Vector3> parentMap, Vector3 start, Vector3 end)
        {
            List<Vector3> path = new();
            Vector3 currentNode = end;

            while (currentNode != start)
            {
                path.Add(currentNode);
                currentNode = parentMap[currentNode];
            }

            path.Add(start);
            path.Reverse();
            return path;
        }

        /// <summary>
        /// Draws gizmos in the Unity editor to visualize the start node, end node, discovered nodes, and the current path.
        /// </summary>
        void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(startNode, .3f);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(endNode, .3f);

            if (discovered != null && showDiscovered)
            {
                foreach (var node in discovered)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(node, .3f);
                }
            }

            if (path != null && showPath)
            {
                foreach (var node in path)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(node, .3f);
                }
            }

#if UNITY_EDITOR
            if (costDictionary.Count != 0 && showCost)
            {
                foreach (var kvp in costDictionary)
                {
                    Gizmos.color = Color.white;
                    Handles.Label(new(kvp.Key.x, kvp.Key.y + .5f, kvp.Key.z), kvp.Value.ToString());
                }
            }
#endif
        }
    }
}
