using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon.Generation
{
    using DataStructures;
    using System;
    using System.Linq;

    /// TODO: 
    /// - Rework the RemoveRandomRooms method to remove 10% of the rooms, smallest first. Stopping if it disconnects the dungeon
    /// - Rework the system so a graph is made BEFORE removing rooms, then remove rooms until before the graph is fully disconnected.

    /// <summary>
    /// Class thats responsible for generating a dungeon. It makes use of <see cref="Room">Room</see> and <see cref="Graph{T}">Graph</see> classes.
    /// <para> Settings set in <see cref="GenerationSettings"> Generation Settings</see></para>
    /// </summary>
    public class DungeonGeneration : MonoBehaviour
    {
        [Header("Dungeon Settings")]
        [SerializeField] private Vector2Int dungeonSize = new(100, 100);
        [SerializeField] private GenerationSettings generationSettings;
        [Space(10), HorizontalLine(height: 1)]
        [Header("Visualization")]
        [SerializeField] private bool drawDungeon;
        [SerializeField] private bool drawDoors;
        [SerializeField] private bool drawDeletedRooms;
        [SerializeField] private bool drawUnreachableRooms;
        [SerializeField] private bool drawStarterRoom;
        [SerializeField] private bool drawGraph;
        [Space(10), HorizontalLine(height: 1)]
        [Header("Debugging Lists")]
        [SerializeField] private List<Room> toSplitRooms = new();
        [SerializeField] private List<Room> toDrawRooms = new();
        [SerializeField] private List<Room> deletedRooms = new();
        [SerializeField] private List<Room> unreachableRooms = new();
        [SerializeField] private List<RectInt> doors = new();
        [SerializeField] private List<Graph<Room>> graphs = new();
        [HorizontalLine(height: 1)]

        private int _mainGraphIndex;
        private bool _mainGraphFound;

        private Random _random;

        /// <summary>
        /// Coroutine that starts the dungeon generation process.
        /// </summary>
        /// <returns>IEnumerator for coroutine.</returns>
        [Button("Start Dungeon Generation", EButtonEnableMode.Playmode)]
        private IEnumerator Start()
        {
            Reset();

            _random = new Random(generationSettings.seed);
            CreateOuterBounds();
            if (generationSettings.useSelfMadeOrder)
                yield return StartCoroutine(OwnOrder());
            else
                yield return StartCoroutine(AssignementOrder());
        }

        private IEnumerator AssignementOrder()
        {
            yield return SplitRooms();
            yield return new WaitForSeconds(1);
            yield return CreateGraph();
            yield return new WaitForSeconds(1);
            yield return RemoveSmallestRooms();
        }

        private IEnumerator OwnOrder()
        {
            yield return new WaitForSeconds(1);
            yield return SplitRooms();
            yield return new WaitForSeconds(1);
            yield return RemoveRandomRooms();
            yield return new WaitForSeconds(1);
            yield return CreateGraph();
            yield return FilterGraphs();
        }

        /// <summary>
        /// Resets the dungeon generation process.
        /// </summary>
        private void Reset()
        {
            toSplitRooms.Clear();
            toDrawRooms.Clear();
            deletedRooms.Clear();
            unreachableRooms.Clear();
            doors.Clear();
            graphs.Clear();
            _mainGraphFound = false;
            _mainGraphIndex = 0;
        }


        /// <summary>
        /// Updates the dungeon visualization every frame.
        /// </summary>
        private void Update()
        {
            Visualization();
        }

        #region Methods

        /// <summary>
        /// Sets the outer bounds of the dungeon. Dictated by <see cref="dungeonSize">dungeonSize</see> variable.
        /// </summary>
        private void CreateOuterBounds()
        {
            toSplitRooms.Add(new(new(0, 0, dungeonSize.x, dungeonSize.y)));
        }

        /// <summary>
        /// Visualizes the rooms in the dungeon.
        /// </summary>
        private void Visualization()
        {
            foreach (Room room in toSplitRooms)
            {
                AlgorithmsUtils.DebugRectInt(room.roomDimensions, Color.cyan);
            }
            if (drawDungeon)
            {
                foreach (Room room in toDrawRooms)
                {
                    AlgorithmsUtils.DebugRectInt(room.roomDimensions, Color.green);
                }
            }
            if (drawDeletedRooms)
            {
                foreach (Room room in deletedRooms)
                {
                    AlgorithmsUtils.DebugRectInt(room.roomDimensions, Color.red);
                }
            }
            if (drawUnreachableRooms)
            {
                foreach (Room room in unreachableRooms)
                {
                    AlgorithmsUtils.DebugRectInt(room.roomDimensions, Color.yellow);
                }
            }
            if (drawDoors && drawDungeon)
            {
                foreach (var door in doors)
                {
                    AlgorithmsUtils.DebugRectInt(door, Color.blue);
                }
            }
            if (drawStarterRoom)
            {
                foreach (Room room in toDrawRooms)
                {
                    if (room.isStartingRoom) AlgorithmsUtils.DebugRectInt(room.roomDimensions, Color.magenta);
                }
            }
            if (drawGraph && _mainGraphFound)
            {
                if (graphs.Count == 0) return;
                VisualizeGraph();
            }
        }

        /// <summary>
        /// Visualize the graph of the dungeon.
        /// </summary>
        private void VisualizeGraph()
        {
            // List to keep track of rooms that have been visualized.
            Dictionary<Room, List<Room>> visualizedRoomPairs = new();

            Graph<Room> graph = graphs[_mainGraphIndex];

            List<Room> rooms = graph.GetNodes();

            // Iterate through each room in the graph.
            foreach (Room room in rooms)
            {
                // Visualize the room with a white rectangle.
                AlgorithmsUtils.DebugRectInt(CalculateOverlayPosition(room), Color.white);

                // Add the room to the list of visualized rooms.
                visualizedRoomPairs.Add(room, new());

                // Get the neighbors of the current room.
                List<Room> neighbors = graph.GetNeighbours(room);

                // Iterate through each neighbor of the current room.
                foreach (Room neighbor in neighbors)
                {
                    // If the neighbor has already been visualized, skip it.
                    if (visualizedRoomPairs.ContainsKey(neighbor)) continue;

                    // Visualize the neighbor with a black rectangle.
                    AlgorithmsUtils.DebugRectInt(CalculateOverlayPosition(neighbor), Color.white);

                    Vector2 roomCenter = CalculateOverlayPosition(room).center;
                    Vector2 neighbourCenter = CalculateOverlayPosition(neighbor).center;
                    Debug.DrawLine(new(roomCenter.x, 0, roomCenter.y), new(neighbourCenter.x, 0, neighbourCenter.y), Color.white);

                    // Add the neighbor to the list of visualized rooms.
                    visualizedRoomPairs[room].Add(neighbor);
                }
            }
        }

        /// <summary>
        /// Calculates the overlay position of a given room.
        /// </summary>
        /// <param name="room">The room for which to calculate the overlay position.</param>
        /// <returns>A RectInt representing the overlay position of the room.</returns>
        private RectInt CalculateOverlayPosition(Room room)
        {
            RectInt position = room.roomDimensions;
            position.x += position.width / 2;
            position.y += position.height / 2;
            position.width = 1;
            position.height = 1;
            return position;
        }

        /// <summary>
        /// Coroutine that splits the rooms in the dungeon.
        /// </summary>
        /// <returns>IEnumerator for coroutine.</returns>
        private IEnumerator SplitRooms()
        {
            while (toSplitRooms.Count > 0)
            {
                Room toSplitRoom = toSplitRooms.Pop(0);

                int minSize = _random.Next(generationSettings.minRoomSize.x, generationSettings.minRoomSize.y);
                //int minSize = 10;

                //Debug.Log("minSize:\t" + minSize);

                Debug.Log("Currently want to split " + toSplitRoom);

                if (toSplitRoom.roomDimensions.width / 2 < minSize && toSplitRoom.roomDimensions.height / 2 < minSize)
                {
                    Debug.Log("Current room to split is too small to split further. Current size is " + toSplitRoom);
                    toDrawRooms.Add(toSplitRoom);
                    continue;
                }

                // Get the direction to split the room into.
                Direction direction = toSplitRoom.roomDimensions.height >= toSplitRoom.roomDimensions.width ? Direction.Horizontal : Direction.Vertical;

                SplitRoom(toSplitRoom.roomDimensions, direction, minSize);

                yield return Delay(generationSettings.delaySettings.RoomGeneration);
            }

            /// <summary>
            /// Splits the rooms in the dungeon.
            /// </summary>
            /// <param name="toSplit">The room to be split.</param>
            /// <param name="direction">The direction to split the room.</param>
            /// <param name="minSize">The minimum size of the split rooms.</param>
            void SplitRoom(RectInt toSplit, Direction direction, int minSize)
            {

                Room splitRoomA = new(RectInt.zero);
                Room splitRoomB = new(RectInt.zero);

                // If the direction decided was vertical, then cut the room vertically
                if (direction == Direction.Vertical)
                {
                    int width = _random.Next(minSize, toSplit.width - minSize);

                    splitRoomA.roomDimensions = new(toSplit.x, toSplit.y, width + 1, toSplit.height);
                    splitRoomB.roomDimensions = new(toSplit.x + width, toSplit.y, toSplit.width - width, toSplit.height);
                    toSplitRooms.Add(splitRoomA);
                    toSplitRooms.Add(splitRoomB);
                }
                // if not vertical, then cut the room horizontally
                else
                {
                    int height = _random.Next(minSize, toSplit.height - minSize);

                    splitRoomA.roomDimensions = new(toSplit.x, toSplit.y, toSplit.width, height + 1);
                    splitRoomB.roomDimensions = new(toSplit.x, toSplit.y + height, toSplit.width, toSplit.height - height);
                    toSplitRooms.Add(splitRoomA);
                    toSplitRooms.Add(splitRoomB);
                }
            }
        }


        /// <summary>
        /// Coroutine that removes a random amount of rooms from the dungeon.
        /// </summary>
        /// <returns></returns>
        private IEnumerator RemoveRandomRooms()
        {
            // Calculate the maximum amount of rooms to remove, clamped between 0 and the amount of rooms to draw so it doesn't accidentally go negative.
            int maxRemovalAmount = (int)(toDrawRooms.Count * (generationSettings.maxRemovalAmount / 100f));
            maxRemovalAmount = Mathf.Clamp(maxRemovalAmount, 0, toDrawRooms.Count);

            // Get the amount of rooms to remove, a value between 0 and maxRemovalAmount.
            int removeAmount = generationSettings.removeMaxRooms ? maxRemovalAmount : _random.Next(0, maxRemovalAmount);

            for (int i = 0; i < removeAmount; i++)
            {
                int index = _random.Next(0, toDrawRooms.Count);
                deletedRooms.Add(toDrawRooms.Pop(index));
                yield return Delay(generationSettings.delaySettings.RoomRemoval);
            }
        }

        /// <summary>
        /// Removes the smallest rooms from the dungeon until 10% of the rooms are removed or the dungeon is about to be disconnected.
        /// </summary>
        /// <returns>IEnumerator for coroutine.</returns>
        private IEnumerator RemoveSmallestRooms()
        {
            /// TODO:
            /// - Profile the system to see how fast linq is, and then make a attempt to do it myself. Profile self-made system to see if it is faster.
            ///   If it is faster then the LINQ system, then keep the self-made system.

            // Sorting the list using LINQ, in order of size, smallest to largest.
            List<Room> orderedRooms = toDrawRooms.OrderBy(x => x.roomDimensions.width * x.roomDimensions.height).ToList();

        }

        /// <summary>
        /// Builds the doors between rooms using Depth-First Search (DFS) to ensure each door is created only once.
        /// </summary>
        private IEnumerator CreateDoors(int graphIndex)
        {
            Graph<Room> startGraph = graphs[graphIndex];
            Graph<Room> graphWithDoors = new();
            List<Room> rooms = startGraph.GetNodes();
            HashSet<Room> visited = new();
            Stack<Room> stack = new();
            int doorSize = generationSettings.doorSize;

            if (rooms.Count == 0) yield break;

            stack.Push(rooms[0]);

            while (stack.Count > 0)
            {
                Room current = stack.Pop();
                if (!visited.Add(current)) continue;

                // Mark the room as visited.

                // Add the room to the graph with doors.
                graphWithDoors.AddNode(current);

                List<Room> neighbors = startGraph.GetNeighbours(current);

                foreach (Room neighbor in neighbors)
                {
                    if (!visited.Contains(neighbor))
                    {
                        RectInt intersectArea = AlgorithmsUtils.Intersect(current.roomDimensions, neighbor.roomDimensions);

                        Room door = new(CreateDoor(intersectArea, doorSize));
                        doors.Add(door.roomDimensions);

                        graphWithDoors.AddNode(neighbor);
                        graphWithDoors.AddNode(door);

                        graphWithDoors.AddEdge(current, door);
                        graphWithDoors.AddEdge(door, neighbor);

                        stack.Push(neighbor);

                        yield return Delay(generationSettings.delaySettings.DoorCreation);
                    }
                }
            }

            graphs[graphIndex] = graphWithDoors;
        }

        /// <summary>
        /// Creates a door between two rooms.
        /// </summary>
        /// <param name="intersectArea">The area where the two rooms intersect.</param>
        /// <param name="doorSize">The size of the door to create.</param>
        /// <returns>A RectInt representing the door's dimensions and position.</returns>
        private RectInt CreateDoor(RectInt intersectArea, int doorSize)
        {
            if (intersectArea.width < intersectArea.height)
            {
                intersectArea.y += _random.Next(1, intersectArea.height - doorSize - 1);
                intersectArea.height = doorSize;
                doors.Add(intersectArea);
                return intersectArea;
            }
            else
            {
                intersectArea.x += _random.Next(1, intersectArea.width - doorSize - 1);
                intersectArea.width = doorSize;
                doors.Add(intersectArea);
                return intersectArea;
            }
        }

        /// <summary>
        /// Filters the graphs to remove any unreachable rooms.
        /// </summary>
        private IEnumerator FilterGraphs()
        {
            int highestRoomCountGraphIndex = 0;

            for (int i = 0; i < graphs.Count; i++)
            {
                if (graphs[i].GetNodeCount() >= graphs[highestRoomCountGraphIndex].GetNodeCount())
                {
                    highestRoomCountGraphIndex = i;
                }
            }

            for (int i = 0; i < graphs.Count; i++)
            {
                if (i == highestRoomCountGraphIndex) continue;

                List<Room> rooms = graphs[i].GetNodes();

                foreach (Room room in rooms)
                {
                    unreachableRooms.Add(toDrawRooms.Pop(toDrawRooms.IndexOf(room)));
                }
            }

            yield return Delay(generationSettings.delaySettings.GraphFiltering);
            yield return CreateDoors(highestRoomCountGraphIndex);

            _mainGraphIndex = highestRoomCountGraphIndex;
            _mainGraphFound = true;
        }


        /// <summary>
        /// Creates the graph of the dungeon.
        /// </summary>
        private IEnumerator CreateGraph()
        {
            List<Room> toCheck = new(toDrawRooms);

            while (toCheck.Count > 0)
            {
                if (generationSettings.minimumDoorCreation)
                {
                    yield return CreateMinimumDoorGraph(toCheck);
                }
                else
                {
                    yield return CreateMultipleDoorGraph(toCheck);
                }
            }
        }

        /// <summary>
        /// Creates a graph with multiple doors between rooms.
        /// </summary>
        private IEnumerator CreateMultipleDoorGraph(List<Room> toCheck)
        {
            /// TODO: Implement the multiple door spawning system. 
            /// First door is guaranteed to spawn, then the rest are by chance, which lowers with each door added.


            // Graph to store the connections in
            Graph<Room> connections = new();
            // List 
            List<Room> connectedRooms = new();

            int doorSize = generationSettings.doorSize;

            // Add the first room to the graph.
            connectedRooms.Add(toCheck.Pop(0));
            connectedRooms[0].isStartingRoom = true;
            connectedRooms[0].isConnected = true;

            for (int i = 0; i < connectedRooms.Count; i++)
            {
                for (int j = 0; j < toCheck.Count; j++)
                {
                    // If the rooms do not intersect or are the same room, continue.
                    if (!AlgorithmsUtils.Intersects(connectedRooms[i].roomDimensions, toCheck[j].roomDimensions)
                        || connectedRooms[i].roomDimensions == toCheck[j].roomDimensions) continue;

                    RectInt intersectArea = AlgorithmsUtils.Intersect(connectedRooms[i].roomDimensions, toCheck[j].roomDimensions);

                    // If the intersect area is too small to place a door, continue.
                    if (intersectArea.width < doorSize + 2 && intersectArea.height < doorSize + 2) continue;

                    connections.AddNode(connectedRooms[i]);
                    connections.AddNode(toCheck[j]);

                    connections.AddEdge(connectedRooms[i], toCheck[j]);

                    connectedRooms.Add(toCheck.Pop(j));
                    j--;
                }
                yield return Delay(generationSettings.delaySettings.GraphCreation);
            }

            // If the key doesn't exist, then we add it to the connections graph.
            if (connections.GetNeighbours(connectedRooms[0]) == null)
            {
                connections.AddNode(connectedRooms[0]);
            }

            graphs.Add(connections);
        }

        /// <summary>
        /// Creates a graph with the minimum amount of doors between rooms.
        /// </summary>
        private IEnumerator CreateMinimumDoorGraph(List<Room> toCheck)
        {
            // Create a graph to store the connections between rooms.
            Graph<Room> connections = new();
            // Create a list for rooms that have been added to the connections graph
            List<Room> connectedRooms = new();

            int doorSize = generationSettings.doorSize;

            // Add the first room to the graph.
            connectedRooms.Add(toCheck.Pop(0));
            connectedRooms[0].isStartingRoom = true;
            connectedRooms[0].isConnected = true;

            for (int i = 0; i < connectedRooms.Count; i++)
            {
                for (int j = 0; j < toCheck.Count; j++)
                {
                    // If the rooms do not intersect or are the same room, continue.
                    if (!AlgorithmsUtils.Intersects(connectedRooms[i].roomDimensions, toCheck[j].roomDimensions)
                        || connectedRooms[i].roomDimensions == toCheck[j].roomDimensions) continue;

                    RectInt intersectArea = AlgorithmsUtils.Intersect(connectedRooms[i].roomDimensions, toCheck[j].roomDimensions);

                    // If the intersect area is too small to place a door, continue.
                    if (intersectArea.width < doorSize + 2 && intersectArea.height < doorSize + 2) continue;

                    connections.AddNode(connectedRooms[i]);
                    connections.AddNode(toCheck[j]);

                    connections.AddEdge(connectedRooms[i], toCheck[j]);

                    connectedRooms.Add(toCheck.Pop(j));
                    j--;
                }
                yield return Delay(generationSettings.delaySettings.GraphCreation);
            }

            // If the key doesn't exist, then we add it to the connections graph.
            if (connections.GetNeighbours(connectedRooms[0]) == null)
            {
                connections.AddNode(connectedRooms[0]);
            }

            graphs.Add(connections);
        }

        /// <summary>
        /// Delays the execution based on the specified delay type.
        /// </summary>
        /// <param name="delayType">The type of delay to apply (Instant, Delayed, KeyPress).</param>
        /// <returns>An IEnumerator for the coroutine.</returns>
        private IEnumerator Delay(DelayType delayType)
        {
            if (!generationSettings.delaySettings.UseDefaultDelayType)
            {
                switch (delayType)
                {
                    case DelayType.Instant:
                        break;
                    case DelayType.Delayed:
                        yield return new WaitForSeconds(generationSettings.delaySettings.actionDelay);
                        break;
                    case DelayType.KeyPress:
                        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
                        break;
                }
            }
            else
            {
                switch (generationSettings.delaySettings.defaultDelayType)
                {
                    case DelayType.Instant:
                        break;
                    case DelayType.Delayed:
                        yield return new WaitForSeconds(generationSettings.delaySettings.actionDelay);
                        break;
                    case DelayType.KeyPress:
                        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
                        break;
                }
            }
        }
        #endregion Methods
    }

    /// <summary>
    /// Enum representing the possible directions for room splitting.
    /// </summary>
    internal enum Direction
    {
        Vertical,
        Horizontal
    }
}
