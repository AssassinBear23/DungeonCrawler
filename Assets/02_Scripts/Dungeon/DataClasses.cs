using NaughtyAttributes;
using System;
using UnityEngine;

namespace Dungeon.DataStructures
{
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
    /// Represents the settings for delays in dungeon generation operations.
    /// </summary>
    /// <remarks>
    /// This class allows customization of delays for various dungeon generation actions.
    /// Users can specify a default delay type or configure individual delay types for specific actions.
    /// </remarks>
    [Serializable]
    public class DelaySettings
    {
        [Tooltip("The time between operations.")]
        [Range(0.01f, 10f)]
        public float actionDelay = .5f;
        [Tooltip("If true, the default delay type will be used for all actions.")]
        [field: SerializeField] public bool UseDefaultDelayType { get; set; }
        [ShowIf("UseDefaultDelayType"), Tooltip("The default delay type to use"), AllowNesting]
        public DelayType defaultDelayType;
        [HideIf("UseDefaultDelayType"), AllowNesting]
        public DelayType RoomGeneration;
        [HideIf("UseDefaultDelayType"), AllowNesting]
        public DelayType GraphCreation;
        [HideIf("UseDefaultDelayType"), AllowNesting]
        public DelayType RoomRemoval;
        [HideIf("UseDefaultDelayType"), AllowNesting]
        public DelayType DoorCreation;
        [HideIf("UseDefaultDelayType"), AllowNesting]
        public DelayType FloorPlacement;
        [HideIf("UseDefaultDelayType"), AllowNesting]
        public DelayType WallPlacement;
        [HideIf("UseDefaultDelayType"), AllowNesting]
        public DelayType PathfindingGraphCreation;
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
        [HorizontalLine]
        public DelaySettings delaySettings;
    }

    /// <summary>
    /// Specifies the type of delay to be applied during dungeon generation actions.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>
    /// <term>Instant</term>
    /// <description>No delay is applied; the action occurs immediately.</description>
    /// </item>
    /// <item>
    /// <term>Delayed</term>
    /// <description>A fixed time delay is applied before the action occurs.</description>
    /// </item>
    /// <item>
    /// <term>KeyPress</term>
    /// <description>The action occurs only after a user key press.</description>
    /// </item>
    /// </list>
    /// </remarks>
    public enum DelayType
    {
        /// <summary>
        /// No delay; the action is performed instantly.
        /// </summary>
        Instant,
        /// <summary>
        /// The action is performed after a specified delay.
        /// </summary>
        Delayed,
        /// <summary>
        /// The action is performed after a user key press.
        /// </summary>
        KeyPress
    }

    /// <summary>
    /// Enum representing the possible directions for room splitting.
    /// </summary>
    [Flags]
    internal enum Direction
    {
        Vertical = 1 << 0,
        Horizontal = 1 << 1
    }
}