using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGeneration : MonoBehaviour
{
    [SerializeField] private RectInt startingRoom = new(0, 0, 100, 50);
    [SerializeField] private Vector2Int minRoomSize = new(10, 10);
    [SerializeField] private int doorSize = 3;

    [SerializeField] private float splittingSpeed = .5f;

    [SerializeField] private List<RectInt> toSplitRooms = new();
    [SerializeField] private List<RectInt> toDrawRooms = new();

    /// <summary>
    /// Coroutine that starts the dungeon generation process.
    /// </summary>
    /// <returns>IEnumerator for coroutine.</returns>
    private IEnumerator Start()
    {
        CreateStartingStructure();
        yield return new WaitForSeconds(1);
        yield return StartCoroutine(SplitRooms());
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
    /// Creates the initial structure of the dungeon.
    /// </summary>
    private void CreateStartingStructure()
    {
        toSplitRooms.Add(startingRoom);
    }

    /// <summary>
    /// Visualizes the rooms in the dungeon.
    /// </summary>
    private void VisualizeRooms()
    {
        foreach (var room in toDrawRooms)
        {
            AlgorithmsUtils.DebugRectInt(room, Color.green);
        }
        foreach (var room in toSplitRooms)
        {
            AlgorithmsUtils.DebugRectInt(room, Color.red);
        }
    }

    /// <summary>
    /// Splits the rooms in the dungeon.
    /// </summary>
    private void SplitRoom(RectInt toSplit, Direction direction, int minSize)
    {

        RectInt splitRoomA;
        RectInt splitRoomB;

        // If the direction decided was vertical, then cut the room vertically
        if (direction == Direction.Vertical)
        {
            int width = Random.Range(minSize, toSplit.width - minSize);

            splitRoomA = new(toSplit.x, toSplit.y, width + 1, toSplit.height);
            splitRoomB = new(toSplit.x + width, toSplit.y, toSplit.width - width, toSplit.height);
            toSplitRooms.Add(splitRoomA);
            toSplitRooms.Add(splitRoomB);
        }
        // if not vertical, then cut the room horizontally
        else
        {
            int height = Random.Range(minSize, toSplit.height - minSize);

            splitRoomA = new(toSplit.x, toSplit.y, toSplit.width, height + 1);
            splitRoomB = new(toSplit.x, toSplit.y + height, toSplit.width, toSplit.height - height);
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
            RectInt toSplitRoom = toSplitRooms.Pop(0);

            int minSize = Random.Range(minRoomSize.x, minRoomSize.y);
            //int minSize = 10;

            //Debug.Log("minSize:\t" + minSize);

            Debug.Log("Currently want to split " + toSplitRoom);

            if (toSplitRoom.width / 2 < minSize && toSplitRoom.height / 2 < minSize)
            {
                Debug.Log("Current room to split is too small to split further. Current size is " + toSplitRoom);
                toDrawRooms.Add(toSplitRoom);
                continue;
            }

            // Get the direction to split the room into.
            Direction direction;

            if (toSplitRoom.height >= toSplitRoom.width)
            {
                direction = Direction.Horizontal;
            }
            else
            {
                direction = Direction.Vertical;
            }

            SplitRoom(toSplitRoom, direction, minSize);

            yield return new WaitForSeconds(splittingSpeed);

        }
    }
    #endregion
}

/// <summary>
/// Enum representing the possible directions for room splitting.
/// </summary>
enum Direction
{
    Vertical,
    Horizontal
}