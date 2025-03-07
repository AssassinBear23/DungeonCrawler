using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [Space(10), HorizontalLine(height: 1)]
    [Header("Debugging Lists")]
    [SerializeField] private List<Room> toSplitRooms = new();
    [SerializeField] private List<Room> toDrawRooms = new();
    [SerializeField] private List<Room> deletedRooms = new();
    [SerializeField] private List<Room> unreachableRooms = new();
    [SerializeField] private List<RectInt> doors = new();
    [SerializeField] private List<Graph<Room>> graphs = new();


    private System.Random random;

    /// <summary>
    /// Coroutine that starts the dungeon generation process.
    /// </summary>
    /// <returns>IEnumerator for coroutine.</returns>
    private IEnumerator Start()
    {
        random = new System.Random(generationSettings.seed);
        CreateOuterBounds();
        yield return new WaitForSeconds(1);
        yield return StartCoroutine(SplitRooms());
        yield return new WaitForSeconds(1);
        yield return StartCoroutine(RemoveRandomRooms());
        yield return new WaitForSeconds(1);
        CreateGraph();
        FilterGraphs();

    }



    /// <summary>
    /// Updates the dungeon visualization every frame.
    /// </summary>
    void Update()
    {
        VisualizeRooms();
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
    private void VisualizeRooms()
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
        if (drawDoors)
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
    }

    /// <summary>
    /// Splits the rooms in the dungeon.
    /// </summary>
    /// <param name="toSplit">The room to be split.</param>
    /// <param name="direction">The direction to split the room.</param>
    /// <param name="minSize">The minimum size of the split rooms.</param>
    private void SplitRoom(RectInt toSplit, Direction direction, int minSize)
    {

        Room splitRoomA = new(RectInt.zero);
        Room splitRoomB = new(RectInt.zero);

        // If the direction decided was vertical, then cut the room vertically
        if (direction == Direction.Vertical)
        {
            int width = random.Next(minSize, toSplit.width - minSize);

            splitRoomA.roomDimensions = new(toSplit.x, toSplit.y, width + 1, toSplit.height);
            splitRoomB.roomDimensions = new(toSplit.x + width, toSplit.y, toSplit.width - width, toSplit.height);
            toSplitRooms.Add(splitRoomA);
            toSplitRooms.Add(splitRoomB);
        }
        // if not vertical, then cut the room horizontally
        else
        {
            int height = random.Next(minSize, toSplit.height - minSize);

            splitRoomA.roomDimensions = new(toSplit.x, toSplit.y, toSplit.width, height + 1);
            splitRoomB.roomDimensions = new(toSplit.x, toSplit.y + height, toSplit.width, toSplit.height - height);
            toSplitRooms.Add(splitRoomA);
            toSplitRooms.Add(splitRoomB);
        }
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

            int minSize = random.Next(generationSettings.minRoomSize.x, generationSettings.minRoomSize.y);
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
            Direction direction;

            if (toSplitRoom.roomDimensions.height >= toSplitRoom.roomDimensions.width)
            {
                direction = Direction.Horizontal;
            }
            else
            {
                direction = Direction.Vertical;
            }

            SplitRoom(toSplitRoom.roomDimensions, direction, minSize);

            yield return new WaitForSeconds(generationSettings.splittingSpeed);

        }
    }


    /// <summary>
    /// Coroutine that removes a random amount of rooms from the dungeon.
    /// </summary>
    /// <returns></returns>
    private IEnumerator RemoveRandomRooms()
    {
        // Calculate the maximum amount of rooms to remove, clamped between 0 and the amount of rooms to draw so it doesnt accidentally go negative.
        int maxRemovalAmount = (int)(toDrawRooms.Count * (generationSettings.maxRemovalAmount / 100f));
        maxRemovalAmount = Mathf.Clamp(maxRemovalAmount, 0, toDrawRooms.Count);

        // Get the amount of rooms to remove, a value between 0 and maxRemovalAmount.
        int removeAmount;
        if (generationSettings.removeMaxRooms)
        {
            removeAmount = maxRemovalAmount;
        }
        else
        {
            removeAmount = random.Next(0, maxRemovalAmount);
        }

        for (int i = 0; i < removeAmount; i++)
        {
            int index = random.Next(0, toDrawRooms.Count);
            deletedRooms.Add(toDrawRooms.Pop(index));
            yield return new WaitForSeconds(generationSettings.splittingSpeed / 10);
        }
    }

    /// <summary>
    /// Builds the doors between two rooms.
    /// </summary>
    /// <param name="intersectArea">The area where the two rooms intersect.</param>
    private RectInt BuildDoor(RectInt intersectArea, int doorSize)
    {
        if (intersectArea.width < intersectArea.height)
        {
            intersectArea.y += random.Next(1, intersectArea.height - doorSize - 1);
            intersectArea.height = doorSize;
            doors.Add(intersectArea);
            return intersectArea;
        }
        else
        {
            intersectArea.x += random.Next(1, intersectArea.width - doorSize - 1);
            intersectArea.width = doorSize;
            doors.Add(intersectArea);
            return intersectArea;
        }
    }

    /// <summary>
    /// Creates the graph of the dungeon.
    /// </summary>
    private void CreateGraph()
    {
        List<Room> toCheck = new(toDrawRooms);

        while (toCheck.Count > 0)
        {
            if (generationSettings.minimumDoorCreation)
            {
                graphs.Add(CreateMinimumDoorGraph(toCheck));
            }
            else
            {
                graphs.Add(CreateMultipleDoorGraph(toCheck));
            }
        }
    }

    /// <summary>
    /// Filters the graphs to remove any unreachable rooms.
    /// </summary>
    private void FilterGraphs()
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
                if (room.isDoor) continue;
                unreachableRooms.Add(toDrawRooms.Pop(toDrawRooms.IndexOf(room)));
            }
        }
    }

    /// <summary>
    /// Creates a graph with multiple doors between rooms.
    /// </summary>
    private Graph<Room> CreateMultipleDoorGraph(List<Room> toCheck)
    {
        Graph<Room> connections = new();
        return connections;
    }

    /// <summary>
    /// Creates a graph with the minimum amount of doors between rooms.
    /// </summary>
    private Graph<Room> CreateMinimumDoorGraph(List<Room> toCheck)
    {
        // Create a graph to store the connections between rooms.
        Graph<Room> connections = new();
        // Create a list of rooms that are eligible to have doors between them.
        List<Room> eligbleRooms = new();

        int doorSize = generationSettings.doorSize;

        // Add the first room to the graph.
        eligbleRooms.Add(toCheck.Pop(0));

        for (int i = 0; i < eligbleRooms.Count; i++)
        {
            for (int j = 0; j < toCheck.Count; j++)
            {
                // If the rooms do not intersect or are the same room, continue.
                if (!AlgorithmsUtils.Intersects(eligbleRooms[i].roomDimensions, toCheck[j].roomDimensions)
                    || eligbleRooms[i].roomDimensions == toCheck[j].roomDimensions) continue;

                RectInt intersectArea = AlgorithmsUtils.Intersect(eligbleRooms[i].roomDimensions, toCheck[j].roomDimensions);

                // If the intersect area is too small to place a door, continue.
                if (intersectArea.width < doorSize + 2 && intersectArea.height < doorSize + 2) continue;

                Room door = new(BuildDoor(intersectArea, doorSize), true);
                connections.AddNode(door);
                connections.AddNode(eligbleRooms[i]);
                connections.AddNode(toCheck[j]);

                connections.AddEdge(eligbleRooms[i], door);
                connections.AddEdge(toCheck[j], door);

                eligbleRooms.Add(toCheck.Pop(j));
                j--;
            }
        }

        if(connections.GetNeighbours(eligbleRooms[0]) == null)
        {
            connections.AddNode(eligbleRooms[0]);
        }

        return connections;
    }
    #endregion

    #region data classes
    [Serializable]
    private class GenerationSettings
    {
        [Tooltip("Seed for the random number generator.")]
        public int seed = 0;
        [Tooltip("The minimum and maximum size of a room.")]
        public Vector2Int minRoomSize = new(10, 10);
        [Tooltip("The size of the doors between rooms.")]
        [Range(2, 5)] public int doorSize = 3;
        [Tooltip("The time between operations.")]
        [Space(10)] public float splittingSpeed = .5f;
        [Tooltip("This amount max amount of rooms to remove, if removeMaxRooms is set to true, it will remove this percentage of rooms.")]
        [Range(0, 100)] public int maxRemovalAmount = 50;
        [Tooltip("This boolean decides if you remove the exact amount of rooms or a random amount between 0 and the max amount of rooms to remove.")]
        public bool removeMaxRooms = false;
        [Tooltip("This boolean decides if you want to create the minimum amount of doors between rooms or if doors can have multiple routes to the starting room")]
        public bool minimumDoorCreation = false;
    }

    [Serializable]
    private class Room
    {
        public bool isStartingRoom = false;
        public bool isDoor = false;
        public RectInt roomDimensions;

        public Room(RectInt roomDimensions, bool isDoor = false, bool isStartingRoom = false)
        {
            this.roomDimensions = roomDimensions;
            this.isDoor = isDoor;
            this.isStartingRoom = isStartingRoom;
        }
    }
    #endregion data classes
}

/// <summary>
/// Enum representing the possible directions for room splitting.
/// </summary>ec
enum Direction
{
    Vertical,
    Horizontal
}
