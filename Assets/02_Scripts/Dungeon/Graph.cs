using System;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;

[Serializable]
public class Graph<T>
{
    [SerializeField] private SerializedDictionary<T, List<T>> adjacencies;

    public Graph()
    {
        adjacencies = new();
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

    public int GetNodeCount()
    {
        return adjacencies.Count;
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
    
    public List<T> GetNodes()
    {
        return new List<T>(adjacencies.Keys);
    }
}
