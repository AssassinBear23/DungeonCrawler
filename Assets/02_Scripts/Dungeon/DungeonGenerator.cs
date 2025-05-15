using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon.Generation
{
    using Dungeon.Data;
    using Dungeon.DataStructures;
    using Dungeon.HelperMethods;
    using System;
    using System.Collections;

    public class DungeonGenerator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DungeonDataGenerator dungeonDataGenerator;
        [SerializeField] private TileMapGenerator tileMapGenerator;

        [Header("Prefabs")]
        [SerializeField] private GameObject floorPrefab;
        [SerializeField] private List<GameObject> wallPrefabs;
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
            //CreateWalls();
        }

        /// <summary>
        /// Create the floor of the dungeon using the flood fill algorithm.
        /// </summary>
        private IEnumerator CreateFloor()
        {
            int[,] _tileMap = TileMapGenerator.TileMap;
            Room _starterRoom = dungeonDataGenerator.StarterRoom;

            Vector2Int startTile = new((int)_starterRoom.roomDimensions.center.x, (int)_starterRoom.roomDimensions.center.x);
            bool[,] visited = new bool[_tileMap.GetLength(0), _tileMap.GetLength(1)];
            Queue<Vector2Int> queue = new();

            queue.Enqueue(startTile);

            while (queue.Count > 0)
            {
                Vector2Int currentTile = queue.Dequeue();

                // Check direct neighbors
                CheckNeighbor(_tileMap, new(currentTile.x + 1, currentTile.y), visited, queue);
                CheckNeighbor(_tileMap, new(currentTile.x - 1, currentTile.y), visited, queue);
                CheckNeighbor(_tileMap, new(currentTile.x, currentTile.y + 1), visited, queue);
                CheckNeighbor(_tileMap, new(currentTile.x, currentTile.y - 1), visited, queue);

                if (!visited[currentTile.x, currentTile.y])
                {
                    SpawnFloor(currentTile);
                    visited[currentTile.x, currentTile.y] = true;
                }
                if (!AlgorithmsUtils.DoInstantPass() && DungeonDataGenerator.Instance.GenerationSettings.delaySettings.FloorPlacement != DelayType.Instant)
                    yield return StartCoroutine(AlgorithmsUtils.Delay(DungeonDataGenerator.Instance.GenerationSettings.delaySettings.FloorPlacement));
            }
        }

        /// <summary>
        /// Checks the neighbors of the current tile and adds them to the queue if they are valid.
        /// </summary>
        /// <param name="tileMap">The 2D array representing the dungeon's tile map.</param>
        /// <param name="toCheckPosition">The position of the neighbor tile to check.</param>
        /// <param name="visited">A 2D boolean array indicating whether a tile has been visited.</param>
        /// <param name="queue">The queue used for the flood fill algorithm.</param>
        private void CheckNeighbor(int[,] tileMap, Vector2Int toCheckPosition, bool[,] visited, Queue<Vector2Int> queue)
        {
            if (toCheckPosition.x < 0
                || toCheckPosition.x >= tileMap.GetLength(0)
                || toCheckPosition.y < 0
                || toCheckPosition.y >= tileMap.GetLength(1)) return;

            if (visited[toCheckPosition.x, toCheckPosition.y] == true) return;

            if (queue.Contains(toCheckPosition)) return;

            if (tileMap[toCheckPosition.x, toCheckPosition.y] != 0 && tileMap[toCheckPosition.x, toCheckPosition.y] != 2) return;

            queue.Enqueue(toCheckPosition);
        }

        /// <summary>
        /// Spawns the floor prefab at the specified position.
        /// </summary>
        /// <param name="position">The position to spawn the floor prefab at</param>
        private void SpawnFloor(Vector2Int position)
        {
            if (floorPrefab == null) new ArgumentNullException(nameof(floorPrefab), "Floor prefab is not assigned.");

            Instantiate(floorPrefab, new Vector3(position.x + 0.5f, 0, position.y + 0.5f), Quaternion.identity, parentFloor.transform);
        }


        /// <summary>
        /// Create the walls of the dungeon using binary operations to check which wall needs to go on the tile.
        /// </summary>
        private IEnumerator CreateWalls()
        {
            int[,] _tileMap = TileMapGenerator.TileMap;

            for (int i = 0; i < _tileMap.GetLength(0) - 1; i++)
            {
                for (int j = 0; j < _tileMap.GetLength(1) - 1; j++)
                {
                    if (!AlgorithmsUtils.DoInstantPass() && DungeonDataGenerator.Instance.GenerationSettings.delaySettings.WallPlacement != DelayType.Instant)
                        yield return StartCoroutine(AlgorithmsUtils.Delay(DungeonDataGenerator.Instance.GenerationSettings.delaySettings.WallPlacement));
                    int bitSum = CalculateBitSum(_tileMap, i, j);

                    InstantiateWall(new Vector2(i, j), bitSum);
                }
            }
        }

        private static int CalculateBitSum(int[,] _tileMap, int i, int j)
        {
            return 1 * _tileMap[i, j] + 2 * _tileMap[i + 1, j] + 4 * _tileMap[i, j + 1] + 8 * _tileMap[i + 1, j + 1];
        }

        private void InstantiateWall(Vector2 position, int wallIndex)
        {
            if (wallPrefabs[wallIndex] == null) return;


        }
    }
}