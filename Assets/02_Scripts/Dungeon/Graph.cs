using System.Collections.Generic;
using UnityEngine;

public class Graph<T>
{
    private Dictionary<T, List<T>> adjacencies;

    public Graph()
    {
        adjacencies = new Dictionary<T, List<T>>();
    }

    public void AddNode(T node)
    {
        if (adjacencies.ContainsKey(node))
        {
            Debug.Log("Node already exists in graph");
            return;
        }
        adjacencies[node] = new List<T>();
    }

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

    public List<T> GetNeighbours(T node)
    {
        if (!adjacencies.ContainsKey(node))
        {
            Debug.Log("Node does not exist in graph");
            return null;
        }
        return adjacencies[node];
    }
}
