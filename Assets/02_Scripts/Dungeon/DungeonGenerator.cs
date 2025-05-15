using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon.Generation
{
    using Dungeon.Data;
    using Dungeon.DataStructures;

    public class DungeonGenerator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DungeonDataGenerator dungeonDataGenerator;
        [SerializeField] private TileMapGenerator tileMapGenerator;

        [field: Header("Prefabs")]
        [field: SerializeField] public GameObject FloorPrefab { get; private set; }
        [field: SerializeField] public List<GameObject> WallPrefabs { get; private set; }
        [Space(10)]
        [HorizontalLine(1)]
        [Header("Hierarchy")]
        [Tooltip("Parent object for the walls")]
        [SerializeField] private GameObject parentWalls;
        [Tooltip("Parent object for the floor")]
        [SerializeField] private GameObject parentFloor;

        public void StartGeneration()
        {
            CreateFloor();
            CreateWalls();
        }

        /// <summary>
        /// Create the floor of the dungeon using the flood fill algorithm.
        /// </summary>
        private void CreateFloor()
        {
            int[,] _tileMap = TileMapGenerator.TileMap;
            Room _starterRoom = dungeonDataGenerator.StarterRoom;

            Vector2Int startingPosition = DungeonDataGenerator.CalculateOverlayPosition(_starterRoom).position;


        }

        /// <summary>
        /// Create the walls of the dungeon using binary operations to check which wall needs to go on the tile.
        /// </summary>
        private void CreateWalls()
        {

        }
    }
}