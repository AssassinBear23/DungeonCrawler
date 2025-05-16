using System.Collections.Generic;
using UnityEngine;

namespace Player.Pathfinding
{
    using AYellowpaper.SerializedCollections;
    using Dungeon.DataStructures;

    public class PathFinder : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GraphGenerator graphGenerator;

        [Header("Pathfinding Settings")]
        [SerializeField] private Algorithms toUseAlgorithm = Algorithms.AStar;

        [Header("Debug Variables")]
        [SerializeField] private List<Vector3> path = new List<Vector3>();
        [SerializeField] private Graph<Vector3> graph;
        
        [SerializeField] private Vector3 startNode;
        [SerializeField] private Vector3 endNode;

        HashSet<Vector3> discovered = new HashSet<Vector3>();

        void Start()
        {
            graphGenerator = GetComponent<GraphGenerator>();
            graph = graphGenerator.Graph;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private Vector3 GetClosestNodeToPosition(Vector3 position)
        {
            Vector3 closestNode = Vector3.zero;
            float closestDistance = Mathf.Infinity;

            //Find the closest node to the position

            return closestNode;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public List<Vector3> CalculatePath(Vector3 from, Vector3 to)
        {
            Vector3 playerPosition = from;

            startNode = GetClosestNodeToPosition(playerPosition);
            endNode = GetClosestNodeToPosition(to);

            List<Vector3> shortestPath = new List<Vector3>();

            switch (toUseAlgorithm)
            {
                case Algorithms.Dijkstra:
                    shortestPath = Dijkstra(startNode, endNode);
                    break;
                case Algorithms.AStar:
                    shortestPath = AStar(startNode, endNode);
                    break;
            }

            path = shortestPath; //Used for drawing the path

            return shortestPath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public List<Vector3> Dijkstra(Vector3 start, Vector3 end)
        {
            //Use this "discovered" list to see the nodes in the visual debugging used on OnDrawGizmos()
            discovered.Clear();

            /* */
            return new List<Vector3>(); // No path found
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        List<Vector3> AStar(Vector3 start, Vector3 end)
        {
            //Use this "discovered" list to see the nodes in the visual debugging used on OnDrawGizmos()
            discovered.Clear();




            /* */
            return new List<Vector3>(); // No path found
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public float Cost(Vector3 from, Vector3 to)
        {
            return Vector3.Distance(from, to);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public float Heuristic(Vector3 from, Vector3 to)
        {
            return Vector3.Distance(from, to);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentMap"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        List<Vector3> ReconstructPath(SerializedDictionary<Vector3, Vector3> parentMap, Vector3 start, Vector3 end)
        {
            List<Vector3> path = new List<Vector3>();
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

        void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(startNode, .3f);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(endNode, .3f);

            if (discovered != null)
            {
                foreach (var node in discovered)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(node, .3f);
                }
            }

            if (path != null)
            {
                foreach (var node in path)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(node, .3f);
                }
            }
        }

    }

    /// <summary>
    /// Enum for the algorithms used in the pathfinding
    /// </summary>
    public enum Algorithms
    {
        Dijkstra,
        AStar
    }
}
