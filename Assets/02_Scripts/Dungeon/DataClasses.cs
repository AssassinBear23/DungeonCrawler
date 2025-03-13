using System;
using UnityEngine;


/// <summary>
/// Represents a room in the dungeon.
/// </summary>
[Serializable]
public class Room
{
    public bool isStartingRoom;
    public bool isConnected;
    public RectInt roomDimensions;

    /// <summary>
    /// Initializes a new instance of the <see cref="Room"/> class.
    /// </summary>
    /// <param name="roomDimensions">The dimensions of the room.</param>
    /// <param name="isConnected">Indicates whether the room is connected to the dungeon graph.</param>
    /// <param name="isStartingRoom">Indicates whether the room is the starting room.</param>
    public Room(RectInt roomDimensions, bool isConnected = false, bool isStartingRoom = false)
    {
        this.roomDimensions = roomDimensions;
        this.isConnected = isConnected;
        this.isStartingRoom = isStartingRoom;
    }
}

/// <summary>
/// Contains settings for dungeon generation.
/// </summary>
[Serializable]
public class GenerationSettings
{
    [Tooltip("Seed for the random number generator.")]
    public int seed;
    [Tooltip("The minimum and maximum size of a room.")]
    public Vector2Int minRoomSize = new(10, 10);
    [Tooltip("The size of the doors between rooms.")]
    [Range(2, 5)] public int doorSize = 3;
    [Tooltip("The time between operations.")]
    [Space(10)] public float splittingSpeed = .5f;
    [Tooltip("This amount max amount of rooms to remove, if removeMaxRooms is set to true, it will remove this percentage of rooms.")]
    [Range(0, 100)] public int maxRemovalAmount = 50;
    [Tooltip("This boolean decides if you remove the exact amount of rooms or a random amount between 0 and the max amount of rooms to remove.")]
    public bool removeMaxRooms;
    [Tooltip("This boolean decides if you want to create the minimum amount of doors between rooms or if doors can have multiple routes to the starting room")]
    public bool minimumDoorCreation;
}

