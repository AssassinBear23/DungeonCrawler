using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon.Generation
{
    using Dungeon.Data;
    using Dungeon.DataStructures;
    using System;
    using System.Collections;
    using Utilities;

    /// <summary>
    /// Responsible for generating and visualizing a dungeon in the scene.
    /// Handles floor and wall instantiation based on generated dungeon data and tile maps.
    /// </summary>
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

        [Header("Debug")]
        [SerializeField] private int iViz, jViz = -1;

        /// <summary>
        /// Starts the dungeon generation process by clearing the current dungeon and starting the generation coroutine.
        /// </summary>
        public void StartGeneration()
        {
            ClearCurrent();
            StartCoroutine(StartGen());
        }

        /// <summary>
        /// Coroutine that orchestrates the dungeon generation steps: floor creation, delay, and wall creation.
        /// </summary>
        /// <returns>IEnumerator for coroutine execution.</returns>
        private IEnumerator StartGen()
        {
            yield return StartCoroutine(CreateFloor());
            yield return new WaitForSeconds(DungeonDataGenerator.Instance.GenerationSettings.delaySettings.actionDelay);
            yield return StartCoroutine(CreateWalls());
        }

        /// <summary>
        /// Clears the current dungeon by destroying all child objects of the parent floor and walls.
        /// </summary>
        private void ClearCurrent()
        {
            for (int i = 0; i < parentFloor.transform.childCount; i++)
            {
                Destroy(parentFloor.transform.GetChild(i).gameObject);
            }
            for (int i = 0; i < parentWalls.transform.childCount; i++)
            {
                Destroy(parentWalls.transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Unity Update method. Visualizes the current tile being processed if visualization indices are set.
        /// </summary>
        private void Update()
        {
            if (iViz >= 0 && jViz >= 0)
                VisualizeCurrent(iViz, jViz);
        }

        /// <summary>
        /// Creates the floor of the dungeon using a flood fill algorithm.
        /// </summary>
        /// <returns>IEnumerator for coroutine execution.</returns>
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
                if (!AlgorithmsUtils.DoInstantPass(DungeonDataGenerator.Instance.GenerationSettings.delaySettings.FloorPlacement))
                    yield return StartCoroutine(AlgorithmsUtils.Delay(DungeonDataGenerator.Instance.GenerationSettings.delaySettings.FloorPlacement));
            }
        }

        /// <summary>
        /// Checks the neighbors of the current tile and adds them to the queue if they are valid for floor placement.
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
        /// <param name="position">The position to spawn the floor prefab at.</param>
        private void SpawnFloor(Vector2Int position)
        {
            if (floorPrefab == null) new ArgumentNullException(nameof(floorPrefab), "Floor prefab is not assigned.");

            Instantiate(floorPrefab, new Vector3(position.x + 0.5f, 0, position.y + 0.5f), Quaternion.identity, parentFloor.transform);
        }

        /// <summary>
        /// Creates the walls of the dungeon using binary operations to determine wall placement.
        /// </summary>
        /// <returns>IEnumerator for coroutine execution.</returns>
        private IEnumerator CreateWalls()
        {
            int[,] _tileMap = TileMapGenerator.TileMap;

            for (int i = 0; i < _tileMap.GetLength(0) - 1; i++)
            {
                iViz = i;
                for (int j = 0; j < _tileMap.GetLength(1) - 1; j++)
                {
                    jViz = j;
                    if (!AlgorithmsUtils.DoInstantPass(DungeonDataGenerator.Instance.GenerationSettings.delaySettings.WallPlacement))
                        yield return StartCoroutine(AlgorithmsUtils.Delay(DungeonDataGenerator.Instance.GenerationSettings.delaySettings.WallPlacement));
                    int bitSum = CalculateBitSum(_tileMap, i, j);

                    if (bitSum < 0) continue;

                    InstantiateWall(new Vector2(i, j), bitSum);
                }
            }
            jViz = -1;
            iViz = -1;
        }

        /// <summary>
        /// Calculates a bit sum representing the configuration of tiles for wall placement.
        /// </summary>
        /// <param name="_tileMap">The 2D array representing the dungeon's tile map.</param>
        /// <param name="i">The x index in the tile map.</param>
        /// <param name="j">The y index in the tile map.</param>
        /// <returns>An integer representing the wall configuration, or -1 if invalid.</returns>
        private static int CalculateBitSum(int[,] _tileMap, int i, int j)
        {
            //Debug.Log("\n" + _tileMap[i, j + 1] + "\t" + _tileMap[i + 1, j + 1] + "\n" + _tileMap[i, j] + "\t" + _tileMap[i + 1, j]);
            if (_tileMap[i, j] < 0 || _tileMap[i + 1, j] < 0 || _tileMap[i, j + 1] < 0 || _tileMap[i + 1, j + 1] < 0) return -1;
            return 1 * _tileMap[i, j + 1]
                   + 2 * _tileMap[i + 1, j + 1]
                   + 4 * _tileMap[i, j]
                   + 8 * _tileMap[i + 1, j];
        }

        /// <summary>
        /// Instantiates the appropriate wall prefab at the specified position based on the wall index.
        /// </summary>
        /// <param name="position">The position to spawn the wall prefab at.</param>
        /// <param name="wallIndex">The index of the wall prefab to use.</param>
        private void InstantiateWall(Vector2 position, int wallIndex)
        {
            if (wallPrefabs[wallIndex] == null) return;

            Instantiate(wallPrefabs[wallIndex], new Vector3(position.x, 0, position.y), Quaternion.identity, parentWalls.transform);
        }

        /// <summary>
        /// Visualizes the current tile being processed in the editor for debugging purposes.
        /// </summary>
        /// <param name="iViz">The x index of the tile to visualize.</param>
        /// <param name="jViz">The y index of the tile to visualize.</param>
        private void VisualizeCurrent(int iViz, int jViz)
        {
            AlgorithmsUtils.DebugRectInt(new RectInt(iViz, jViz, 2, 2), Color.red, height: 3);
        }
    }
}