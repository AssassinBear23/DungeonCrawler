using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Dungeon.DataStructures
{
    /// <summary>
    /// Represents a priority queue data structure that supports generic types.
    /// </summary>
    /// <typeparam name="T">The type of element to use for the value.</typeparam>
    [Serializable]
    public class PriorityQueue<T>
    {
        /// <summary>
        /// Stores the elements and their associated priority values.
        /// </summary>
        [SerializeField] private Dictionary<T, float> keyPriorityPairs;

        /// <summary>
        /// Initializes a new instance of the <see cref="PriorityQueue{T}"/> class.
        /// </summary>
        public PriorityQueue()
        {
            keyPriorityPairs = new();
        }

        /// <summary>
        /// Removes all elements from the priority queue.
        /// </summary>
        public void Clear()
        {
            keyPriorityPairs.Clear();
        }

        /// <summary>
        /// Adds an element with a specified priority to the priority queue.
        /// If the element already exists, the method does nothing.
        /// </summary>
        /// <param name="key">The element to add to the queue.</param>
        /// <param name="priority">The priority value associated with the element. Lower values indicate higher priority.</param>
        public void Enqueue(T key, float priority)
        {
            if (keyPriorityPairs.ContainsKey(key))
            {
                return;
            }
            keyPriorityPairs[key] = priority;
        }

        /// <summary>
        /// Sorts the elements in the priority queue by their priority values in ascending order.
        /// </summary>
        private void SortByPriority(bool printToConsole = false)
        {
            keyPriorityPairs = keyPriorityPairs.OrderBy(keyValuePair => keyValuePair.Value).ToDictionary(keyValuePair => keyValuePair.Key, keyValuePair => keyValuePair.Value);
            if (printToConsole)
                PrintDictionary();
        }

        private void PrintDictionary()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var kvp in keyPriorityPairs)
            {
                sb.AppendLine($"Key: {kvp.Key}, Priority: {kvp.Value}");
            }
            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// Removes and returns the element with the highest priority (lowest priority value) from the priority queue.
        /// </summary>
        /// <returns>The element with the highest priority.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the priority queue is empty.</exception>
        public T Dequeue()
        {
            if (keyPriorityPairs.Count == 0)
            {
                throw new InvalidOperationException("The priority queue is empty.");
            }

            SortByPriority();

            var minPair = keyPriorityPairs.First();
            keyPriorityPairs.Remove(minPair.Key);
            return minPair.Key;
        }

        /// <summary>
        /// Removes and returns the element with the highest priority (lowest priority value) from the priority queue.
        /// </summary>
        /// <param name="printToConsole">
        /// If set to <c>true</c>, prints the contents of the priority queue to the console after sorting.
        /// </param>
        /// <returns>
        /// The element with the highest priority.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the priority queue is empty.
        /// </exception>
        public T Dequeue(bool printToConsole)
        {
            if (keyPriorityPairs.Count == 0)
            {
                throw new InvalidOperationException("The priority queue is empty.");
            }

            SortByPriority(printToConsole);

            var minPair = keyPriorityPairs.First();
            keyPriorityPairs.Remove(minPair.Key);
            return minPair.Key;
        }

        /// <summary>
        /// Gets the number of elements in the priority queue.
        /// </summary>
        public int Count => keyPriorityPairs.Count;

        public void UpdatePriority(T key, float newPriority)
        {
            if (!keyPriorityPairs.ContainsKey(key))
                return;

            keyPriorityPairs[key] = newPriority;
        }
    }
}