using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon.Generation
{
    using DataStructures;
    using System;
    using System.Linq;

    /// <summary>
    /// Class thats responsible for generating a dungeon. It makes use of <see cref="Room">Room</see> and <see cref="Graph{T}">Graph</see> classes.
    /// <para> Settings set in <see cref="GenerationSettings"> Generation Settings</see></para>
    /// </summary>
    public class DungeonGenerator : MonoBehaviour
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
        [SerializeField] private Graph<Room> mainGraph = new();
        [HorizontalLine(height: 1)]

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
            yield return StartCoroutine(AssignmentOrder());
        }

        /// <summary>
        /// Creates a new random seed for the dungeon generation process.
        /// </summary>
        [Button("New Random Seed", EButtonEnableMode.Always)]
        private void NewSeed()
        {
            _random = new Random(generationSettings.seed);
            generationSettings.seed = _random.Next(0, 100000);
            Reset();
            CreateOuterBounds();
            StartCoroutine(AssignmentOrder());
        }

        /// <summary>
        /// Executes the order of operations for dungeon generation.
        /// </summary>
        /// <returns></returns>
        private IEnumerator AssignmentOrder()
        {
            yield return SplitRooms();
            yield return new WaitForSeconds(generationSettings.delaySettings.actionDelay);
            yield return StartCoroutine(CreateGraph());
            yield return new WaitForSeconds(generationSettings.delaySettings.actionDelay);
            yield return StartCoroutine(RemoveRooms());
            yield return new WaitForSeconds(generationSettings.delaySettings.actionDelay);
            yield return StartCoroutine(CreateDoors());
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
            mainGraph = new();
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
                VisualizeRoom(room, Color.cyan);
            }
            if (drawDungeon)
            {
                foreach (Room room in toDrawRooms)
                {
                    if (drawStarterRoom)
                        if (room.isStartingRoom) VisualizeRoom(room, Color.magenta);
                        else
                            VisualizeRoom(room, Color.green);
                }
            }
            if (drawDeletedRooms)
            {
                foreach (Room room in deletedRooms)
                {
                    VisualizeRoom(room, Color.red);
                }
            }
            if (drawUnreachableRooms)
            {
                foreach (Room room in unreachableRooms)
                {
                    VisualizeRoom(room, Color.yellow);
                }
            }
            if (drawDoors && drawDungeon)
            {
                foreach (var door in doors)
                {
                    AlgorithmsUtils.DebugRectInt(door, Color.blue);
                }
            }
            if (drawGraph)
            {
                if (mainGraph.GetNodeCount() == 0) return;
                VisualizeGraph(mainGraph);
            }

            static void VisualizeRoom(Room room, Color color)
            {
                AlgorithmsUtils.DebugRectInt(room.roomDimensions, color);
                AlgorithmsUtils.DebugRectInt(new RectInt(room.roomDimensions.x + 1, room.roomDimensions.y + 1, room.roomDimensions.width - 2, room.roomDimensions.height - 2), color);
            }
        }

        /// <summary>
        /// Visualizes the graph of rooms and their connections.
        /// </summary>
        /// <param name="toVisualizeGraph">The graph of rooms to visualize.</param>
        private void VisualizeGraph(Graph<Room> toVisualizeGraph)
        {
            // List to keep track of rooms that have been visualized.
            Dictionary<Room, List<Room>> visualizedRoomPairs = new();

            Graph<Room> graph = toVisualizeGraph;

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

                if (generationSettings.delaySettings.RoomGeneration != DelayType.Instant)
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
        /// Removes the smallest rooms from the dungeon until 10% of the rooms are removed or the dungeon is about to be disconnected.
        /// </summary>
        /// <returns>IEnumerator for coroutine.</returns>
        private IEnumerator RemoveRooms()
        {
            // Sorting the list using LINQ, in order of size, smallest to largest.
            List<Room> orderedRooms = toDrawRooms.OrderBy(x => x.roomDimensions.width * x.roomDimensions.height).ToList();
            int amountOfRoomsToRemove = Mathf.CeilToInt(orderedRooms.Count * 0.1f);

            for (int i = 0; i < amountOfRoomsToRemove; i++)
            {
                Room roomToRemove = orderedRooms[i];
                if (!mainGraph.TryRemoveNode(roomToRemove, toDrawRooms[0]))
                {
                    break;
                }
                deletedRooms.Add(roomToRemove);
                toDrawRooms.Remove(roomToRemove);
                if (generationSettings.delaySettings.RoomRemoval != DelayType.Instant)
                    yield return Delay(generationSettings.delaySettings.RoomRemoval);
            }
        }

        /// <summary>
        /// Builds the doors between rooms using spanning-tree algorithm to ensure each door is created only once.
        /// </summary>
        private IEnumerator CreateDoors()
        {
            Graph<Room> startGraph = mainGraph;             // A copy of the mainGraph.
            Graph<Room> graphWithDoors = new();             // The new graph that will replace the old one, containing the door connections.
            List<Room> rooms = startGraph.GetNodes();       // The rooms in the graph
            HashSet<Room> visited = new();
            //Stack<Room> stack = new();
            Queue<Room> queue = new();
            int doorSize = generationSettings.doorSize;     // The door size to use

            if (rooms.Count == 0) yield break;              // If there are no rooms then stop

            //rooms[0].isConnected = true;
            rooms[0].isStartingRoom = true;
            visited.Add(rooms[0]);                        // Add the first room to the visited set
            //stack.Push(rooms[0]);                         // Pop the first room onto the stack
            queue.Enqueue(rooms[0]);

            while (queue.Count > 0)
            {
                Room current = queue.Dequeue();             // Current room becomes the first on the 
                //if (visited.Contains(current)) continue;
                //visited.Add(current);                       // Add the room to the visited set
                graphWithDoors.AddNode(current);            // Add the room to the graph with doors.

                List<Room> neighbors = startGraph.GetNeighbours(current);

                foreach (Room neighbor in neighbors)
                {
                    if (visited.Contains(neighbor)) continue;

                    RectInt intersectArea = AlgorithmsUtils.Intersect(current.roomDimensions, neighbor.roomDimensions);

                    Room door = new(CreateDoor(intersectArea, doorSize));
                    doors.Add(door.roomDimensions);

                    graphWithDoors.AddNode(neighbor);
                    graphWithDoors.AddNode(door);

                    graphWithDoors.AddEdge(current, door);
                    graphWithDoors.AddEdge(door, neighbor);

                    //neighbor.isConnected = true;
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                }
                if (generationSettings.delaySettings.DoorCreation != DelayType.Instant)
                    yield return Delay(generationSettings.delaySettings.DoorCreation);
            }

            mainGraph = graphWithDoors;
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
        /// Creates the graph of the dungeon.
        /// </summary>
        private IEnumerator CreateGraph()
        {
            List<Room> toCheck = new(toDrawRooms);
            // Graph to store the connections in
            Graph<Room> connections = new();

            int doorSize = generationSettings.doorSize;

            for (int i = 0; i < toCheck.Count; i++)
            {
                for (int j = i + 1; j < toCheck.Count; j++)
                {
                    // If the rooms do not intersect or are the same room, continue.
                    if (!AlgorithmsUtils.Intersects(toCheck[i].roomDimensions, toCheck[j].roomDimensions)
                        || connections.GetNeighbours(toCheck[i]) != null && connections.GetNeighbours(toCheck[i]).Contains(toCheck[j])
                        || i == j) continue;

                    RectInt intersectArea = AlgorithmsUtils.Intersect(toCheck[i].roomDimensions, toCheck[j].roomDimensions);

                    // If the intersect area is too small to place a door, continue.
                    if (intersectArea.width < doorSize + 2 && intersectArea.height < doorSize + 2) continue;


                    connections.AddNode(toCheck[i]);
                    connections.AddNode(toCheck[j]);

                    connections.AddEdge(toCheck[i], toCheck[j]);
                }
                if (drawGraph) VisualizeGraph(connections);
                if (generationSettings.delaySettings.GraphCreation != DelayType.Instant)
                    yield return Delay(generationSettings.delaySettings.GraphCreation);
            }

            // If the key doesn't exist, then we add it to the connections graph.
            if (connections.GetNeighbours(toCheck[0]) == null)
            {
                connections.AddNode(toCheck[0]);
            }

            mainGraph = connections;
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
