using System.Collections;
using UnityEngine;

namespace Player.Pathfinding
{
    using Dungeon.Data;
    using Dungeon.DataStructures;
    using Dungeon.Utilities;

    /// <summary>
    /// Generates a pathfinding graph from a dungeon tile map, supporting both cardinal and diagonal connections.
    /// </summary>
    public class PathfindingGraphGenerator : MonoBehaviour
    {
        public Graph<Vector3> Graph { get; private set; } = new();
        private RectInt dungeonBounds;
        private int[,] tileMap;

        [Header("Debugging")]
        [SerializeField] private bool vizualizeGraph;
        [SerializeField] private int iViz, jViz = -1;
        [SerializeField] private bool graphGenerated = false;

        /// <summary>
        /// Starts the graph generation coroutine.
        /// </summary>
        public void GenerateGraph()
        {
            StartCoroutine(GenGraph());
        }

        /// <summary>
        /// Generates a graph based on the static <see cref="TileMapGenerator.TileMap"/> in <see cref="TileMapGenerator"/>.
        /// Iterates over the dungeon bounds, connecting valid nodes in the graph.
        /// </summary>
        private IEnumerator GenGraph()
        {
            Graph.Clear();
            graphGenerated = false;

            tileMap = TileMapGenerator.TileMap;
            dungeonBounds = GetDungeonBounds();

            for (int x = dungeonBounds.xMin; x < dungeonBounds.xMax; x++)
            {
                iViz = x;
                for (int y = dungeonBounds.yMin; y < dungeonBounds.yMax; y++)
                {
                    jViz = y;
                    Vector3 currentPosition = new(x, 0, y);

                    if (tileMap[x, y] != 0) continue;

                    int tileMapPosX = 0 + (x - dungeonBounds.xMin);
                    int tileMapPosY = 0 + (y - dungeonBounds.yMin);
                    Vector2Int tileMapPosition = new(tileMapPosX, tileMapPosY);

                    TryConnectCardinalNeighbour(0, 1, currentPosition, tileMapPosition);
                    TryConnectCardinalNeighbour(0, -1, currentPosition, tileMapPosition);
                    TryConnectCardinalNeighbour(-1, 0, currentPosition, tileMapPosition);
                    TryConnectCardinalNeighbour(1, 0, currentPosition, tileMapPosition);

                    TryConnectDiagonalNeighbour(-1, +1, currentPosition, Direction.TopLeft, tileMapPosition);
                    TryConnectDiagonalNeighbour(+1, +1, currentPosition, Direction.TopRight, tileMapPosition);
                    TryConnectDiagonalNeighbour(-1, -1, currentPosition, Direction.BottomLeft, tileMapPosition);
                    TryConnectDiagonalNeighbour(+1, -1, currentPosition, Direction.BottomRight, tileMapPosition);

                    if (!AlgorithmsUtils.DoInstantPass(DungeonDataGenerator.Instance.GenerationSettings.delaySettings.PathfindingGraphCreation))
                        yield return StartCoroutine(AlgorithmsUtils.Delay(DungeonDataGenerator.Instance.GenerationSettings.delaySettings.PathfindingGraphCreation));
                }
            }
            iViz = -1;
            jViz = -1;
            graphGenerated = true;
        }

        /// <summary>
        /// Checks if the diagonal neighbour is valid and, if so, adds it to the graph.
        /// </summary>
        /// <param name="neighbourDifferenceX">The X offset to the diagonal neighbor.</param>
        /// <param name="neighbourDifferenceY">The Y offset to the diagonal neighbor.</param>
        /// <param name="currentPosition">The current node position.</param>
        /// <param name="direction">The diagonal direction being checked.</param>
        /// <param name="tileMapPosition">The current position in tile map coordinates.</param>
        private void TryConnectDiagonalNeighbour(int neighbourDifferenceX, int neighbourDifferenceY, Vector3 currentPosition, Direction direction, Vector2Int tileMapPosition)
        {
            Vector3 neighbourPosition = new(neighbourDifferenceX + currentPosition.x, 0, neighbourDifferenceY + currentPosition.z);
            if (tileMap[tileMapPosition.x + neighbourDifferenceX, tileMapPosition.y + neighbourDifferenceY] != 0)
                return;

            if (neighbourPosition.x < dungeonBounds.xMin && (neighbourPosition.x >= dungeonBounds.xMax ||
                neighbourPosition.y < dungeonBounds.yMin) && neighbourPosition.y >= dungeonBounds.yMax)
                return;

            if (IsCardinalBlocked(neighbourPosition, currentPosition, direction))
            {
                Graph.AddNode(currentPosition);
                Graph.AddNode(neighbourPosition);
                Graph.AddEdge(currentPosition, neighbourPosition);
            }
        }

        /// <summary>
        /// Checks if the cardinal neighbours are connected to the original node to see if the diagonal is allowed.
        /// </summary>
        /// <param name="checkingDiagonal">The position of the diagonal neighbor being checked.</param>
        /// <param name="currentPosition">The current node position.</param>
        /// <param name="direction">The diagonal direction being checked.</param>
        /// <returns>True if both required cardinal neighbors are connected; otherwise, false.</returns>
        private bool IsCardinalBlocked(Vector3 checkingDiagonal, Vector3 currentPosition, Direction direction)
        {
            switch (direction)
            {
                case Direction.TopLeft:
                    return
                        Graph.GetNeighbours(currentPosition).Contains(new Vector3(checkingDiagonal.x + 1, 0, checkingDiagonal.z)) &&
                        Graph.GetNeighbours(currentPosition).Contains(new Vector3(checkingDiagonal.x, 0, checkingDiagonal.z - 1));
                case Direction.TopRight:
                    return
                        Graph.GetNeighbours(currentPosition).Contains(new Vector3(checkingDiagonal.x - 1, 0, checkingDiagonal.z)) &&
                        Graph.GetNeighbours(currentPosition).Contains(new Vector3(checkingDiagonal.x, 0, checkingDiagonal.z - 1));
                case Direction.BottomLeft:
                    return
                        Graph.GetNeighbours(currentPosition).Contains(new Vector3(checkingDiagonal.x + 1, 0, checkingDiagonal.z)) &&
                        Graph.GetNeighbours(currentPosition).Contains(new Vector3(checkingDiagonal.x, 0, checkingDiagonal.z + 1));
                case Direction.BottomRight:
                    return
                        Graph.GetNeighbours(currentPosition).Contains(new Vector3(checkingDiagonal.x - 1, 0, checkingDiagonal.z)) &&
                        Graph.GetNeighbours(currentPosition).Contains(new Vector3(checkingDiagonal.x, 0, checkingDiagonal.z + 1));
                default:
                    Debug.LogError($"Invalid direction: {direction}");
                    return false;
            }
        }

        /// <summary>
        /// Attempts to connect a cardinal neighbor (up, down, left, right) to the current node if valid.
        /// </summary>
        /// <param name="neighbourDifferenceX">The X offset to the neighbor.</param>
        /// <param name="neighbourDifferenceY">The Y offset to the neighbor.</param>
        /// <param name="currentPosition">The current node position.</param>
        /// <param name="tileMapPosition">The current position in tile map coordinates.</param>
        private void TryConnectCardinalNeighbour(int neighbourDifferenceX, int neighbourDifferenceY, Vector3 currentPosition, Vector2Int tileMapPosition)
        {
            Vector3 neighbourPosition = new(neighbourDifferenceX + currentPosition.x, 0, neighbourDifferenceY + currentPosition.z);

            if (tileMap[tileMapPosition.x + neighbourDifferenceX, tileMapPosition.y + neighbourDifferenceY] != 0)
                return;

            if (neighbourPosition.x < dungeonBounds.xMin && (neighbourPosition.x >= dungeonBounds.xMax ||
                neighbourPosition.y < dungeonBounds.yMin) && neighbourPosition.y >= dungeonBounds.yMax)
                return;

            Graph.AddNode(currentPosition);
            Graph.AddNode(neighbourPosition);
            Graph.AddEdge(currentPosition, neighbourPosition);
        }

        /// <summary>
        /// Calculates the bounds of the dungeon for graph generation.
        /// </summary>
        /// <returns>The rectangular bounds of the dungeon.</returns>
        private RectInt GetDungeonBounds()
        {
            DungeonDataGenerator dungeonData = DungeonDataGenerator.Instance;

            // Correctly convert Vector3 to Vector2Int by using only the x and y components of the position.  
            Vector2Int transformPos = new((int)dungeonData.transform.position.x, (int)dungeonData.transform.position.y);
            Vector2Int dungeonSize = dungeonData.DungeonSize;

            return new RectInt(transformPos, dungeonSize);
        }

        private void Update()
        {
            if (iViz > 0 && jViz > 0)
                VisualizeCurrent(iViz, jViz);

            if (!graphGenerated || !vizualizeGraph) return;

            float offset = .5f;
            foreach (var node in Graph.GetNodes())
            {
                Vector3 nodeOffset = new(node.x + offset, node.y, node.z + offset);
                DebugExtension.DebugWireSphere(nodeOffset, Color.cyan, .2f);
                foreach (var neighbor in Graph.GetNeighbours(node))
                {
                    Vector3 neighborOffset = new(neighbor.x + offset, neighbor.y, neighbor.z + offset);
                    Debug.DrawLine(nodeOffset, neighborOffset, Color.cyan);
                }
            }
        }

        /// <summary>
        /// Visualizes a 3x3 area around the current node for debugging purposes.
        /// </summary>
        /// <param name="iViz">The X coordinate of the node being visualized.</param>
        /// <param name="jViz">The Y coordinate of the node being visualized.</param>
        private void VisualizeCurrent(int iViz, int jViz)
        {
            AlgorithmsUtils.DebugRectInt(new RectInt(iViz - 1, jViz - 1, 3, 3), Color.green);
        }
    }

    /// <summary>
    /// Represents the four diagonal directions for neighbor checking.
    /// </summary>
    internal enum Direction
    {
        /// <summary>Top left diagonal direction.</summary>
        TopLeft,
        /// <summary>Top right diagonal direction.</summary>
        TopRight,
        /// <summary>Bottom left diagonal direction.</summary>
        BottomLeft,
        /// <summary>Bottom right diagonal direction.</summary>
        BottomRight
    }
}