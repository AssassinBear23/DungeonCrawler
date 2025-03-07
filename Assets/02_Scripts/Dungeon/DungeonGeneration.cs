using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGeneration : MonoBehaviour
{
    [Header("Dungeon Settings")]
    [SerializeField] private Vector2Int dungeonSize = new(100, 100);
    [SerializeField] private GenerationSettings generationSettings;

    [Header("Visualization")]
    [SerializeField] bool drawDungeon;
    [SerializeField] bool drawDoors;
    [SerializeField] bool drawDeletedRooms;
    [SerializeField] bool drawUnreachableRooms;

    [Header("Debugging Lists")]
    [SerializeField] private List<Room> toSplitRooms = new();
    [SerializeField] private List<Room> toDrawRooms = new();
    [SerializeField] private List<Room> deletedRooms = new();
    [SerializeField] private List<Room> unreachableRooms = new();
    [SerializeField] private List<RectInt> doors = new();


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
        StartCoroutine(BuildDoors());
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
    /// Coroutine that builds doors between rooms in the dungeon.
    /// </summary>
    /// <returns>IEnumerator for coroutine.</returns>
    private IEnumerator BuildDoors()
    {


        int doorSize = generationSettings.doorSize;

        for (int i = 0; i < toDrawRooms.Count - 1; i++)
        {
            if (i == 0)
            {
                toDrawRooms[i].isConnected = true;
                toDrawRooms[i].isStartingRoom = true;
            }

            for (int j = i + 1; j < toDrawRooms.Count; j++)
            {
                if (!AlgorithmsUtils.Intersects(toDrawRooms[i].roomDimensions, toDrawRooms[j].roomDimensions)
                    || toDrawRooms[i].roomDimensions == toDrawRooms[j].roomDimensions
                    || toDrawRooms[j].hasDoorsPlaced
                    || toDrawRooms[j].isConnected) continue;

                RectInt intersectArea = AlgorithmsUtils.Intersect(toDrawRooms[i].roomDimensions, toDrawRooms[j].roomDimensions);

                if (intersectArea.width < doorSize + 2 && intersectArea.height < doorSize + 2) continue;

                if (intersectArea.width < intersectArea.height)
                {
                    intersectArea.y += random.Next(1, intersectArea.height - doorSize - 1);
                    intersectArea.height = doorSize;
                    doors.Add(intersectArea);
                }
                else
                {
                    intersectArea.x += random.Next(1, intersectArea.width - doorSize - 1);
                    intersectArea.width = doorSize;
                    doors.Add(intersectArea);
                }

                if (toDrawRooms[i].isConnected) toDrawRooms[j].isConnected = true;

                yield return new WaitForSeconds(generationSettings.splittingSpeed / 10);
            }
            toDrawRooms[i].hasDoorsPlaced = true;
            yield return new WaitForSeconds(generationSettings.splittingSpeed);
        }
    }

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
    }
    #endregion

    #region data classes
    [Serializable]
    private class GenerationSettings
    {
        public int seed = 0;
        public Vector2Int minRoomSize = new(10, 10);
        [Range(2, 5)] public int doorSize = 3;
        [Space(10)] public float splittingSpeed = .5f;
        [Tooltip("This amount max amount of rooms to remove, if removeMaxRooms is set to true, it will remove this percentage of rooms.")]
        [Range(0, 100)] public int maxRemovalAmount = 50;
        [Tooltip("This boolean decides if you remove the exact amount of rooms or a random amount between 0 and the max amount of rooms to remove.")]
        public bool removeMaxRooms = false;
        public bool minimumDoorCreation = false;
    }

    [Serializable]
    private class Room
    {
        public bool isConnected = false;
        public bool hasDoorsPlaced = false;
        public bool isStartingRoom = false;
        public RectInt roomDimensions;

        public Room(RectInt roomDimensions, bool isConnected = false, bool hasDoorsPlaced = false, bool isStartingRoom = false)
        {
            this.roomDimensions = roomDimensions;
            this.isConnected = isConnected;
            this.isStartingRoom = isStartingRoom;
            this.hasDoorsPlaced = hasDoorsPlaced;
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
