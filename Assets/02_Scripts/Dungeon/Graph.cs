using System;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using System.Linq;

[Serializable]
public class Graph<T>
{
    [SerializeField] private SerializedDictionary<T, List<T>> adjacencies;

    /// <summary>
    /// Creates a new graph.
    /// </summary>
    public Graph()
    {
        adjacencies = new();
    }

    /// <summary>
    /// Adds a node to the graph.
    /// </summary>
    /// <param name="node">The node to add to the graph.</param>
    public void AddNode(T node)
    {
        if (adjacencies.ContainsKey(node))
        {
            Debug.Log("Node already exists in graph");
            return;
        }
        adjacencies[node] = new List<T>();
    }

    /// <summary>
    /// Adds an edge between two nodes in the graph.
    /// </summary>
    /// <param name="fromNode">The starting node of the edge.</param>
    /// <param name="toNode">The ending node of the edge.</param>
    public void AddEdge(T fromNode, T toNode)
    {
        if (!adjacencies.ContainsKey(fromNode) || !adjacencies.ContainsKey(toNode))
        {
            Debug.Log("One or both nodes do not exist in graph");
            return;
        }
        adjacencies[fromNode].Add(toNode);
        adjacencies[toNode].Add(fromNode);
    }

    /// <summary>
    /// Gets the number of nodes in the graph.
    /// </summary>
    /// <returns>The number of nodes in the graph.</returns>
    public int GetNodeCount()
    {
        return adjacencies.Count;
    }

    /// <summary>
    /// Returns a list of all nodes adjacent to the given node.
    /// </summary>
    /// <param name="node">The node whose neighbors are to be returned.</param>
    /// <returns>A list of all nodes adjacent to the given node.</returns>
    public List<T> GetNeighbours(T node)
    {
        if (!adjacencies.ContainsKey(node))
        {
            Debug.Log("Node does not exist in graph");
            return null;
        }
        return adjacencies[node];
    }

    /// <summary>
    /// Returns a list of all nodes in the graph.
    /// </summary>
    /// <returns>A list of all nodes in the graph.</returns>
    public List<T> GetNodes()
    {
        return adjacencies.Keys.ToList();
    }

    /// <summary>
    /// Tries to remove a node from the graph. If it splits the graph apart, stop the removal.
    /// </summary>
    /// <param name="nodeToRemove">The node to remove</param>
    /// <returns>True if the room was removed, false if it would split the graph.</returns>
    public bool TryRemoveNode(T nodeToRemove, T startingNode)
    {
        if (!adjacencies.ContainsKey(nodeToRemove))
        {
            return false;
        }

        // Perform DFS to check connectivity
        HashSet<T> visited = new();
        if (startingNode == null)
        {
            return false;
        }

        Stack<T> stack = new();
        stack.Push(startingNode);

        while (stack.Count > 0)
        {
            T current = stack.Pop();
            if (!visited.Add(current))
            {
                continue;
            }

            foreach (T neighbor in adjacencies[current])
            {
                if (!neighbor.Equals(nodeToRemove) && !visited.Contains(neighbor))
                {
                    stack.Push(neighbor);
                }
            }
        }

        // Check if all nodes except the room to remove are visited
        if (visited.Count != adjacencies.Count - 1)
        {
            return false;
        }

        // Remove the room and its references
        foreach (T neighbor in adjacencies[nodeToRemove])
        {
            adjacencies[neighbor].Remove(nodeToRemove);
        }
        adjacencies.Remove(nodeToRemove);

        return true;
    }
}
