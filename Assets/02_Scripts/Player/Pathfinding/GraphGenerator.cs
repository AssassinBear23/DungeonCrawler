using UnityEngine;

namespace Player.Pathfinding
{
    using Dungeon.Data;
    using Dungeon.DataStructures;
    using System.Collections;
    using Utilities;

    public class GraphGenerator : MonoBehaviour
    {
        public Graph<Vector3> Graph { get; private set; } = new();
        RectInt dungeonBounds;
        int[,] tileMap;
        bool graphGenerated = false;

        public void GenerateGraph()
        {
            StartCoroutine(GenGraph());
        }    
        
        /// <summary>
        /// Generates a graph based on public static <see cref="TileMapGenerator.TileMap">TileMap</see> in <see cref="TileMapGenerator">TIleMapGenerator</see>.
        /// </summary>
        private IEnumerator GenGraph()
        {
            Graph.Clear();

            tileMap = TileMapGenerator.TileMap;
            dungeonBounds = GetDungeonBounds();

            for (int x = dungeonBounds.xMin; x < dungeonBounds.xMax; x++)
            {
                for (int y = dungeonBounds.yMin; y < dungeonBounds.yMax; y++)
                {
                    Vector3 currentPosition = new(x, 0, y);

                    // Check if current position is a wall or empty space, if so skip it.
                    if (tileMap[x, y] != 0) continue;

                    int tileMapPosX = 0 + (x - dungeonBounds.xMin);
                    int tileMapPosY = 0 + (y - dungeonBounds.yMin);
                    Vector2Int tileMapPosition = new(tileMapPosX, tileMapPosY);

                    // Check all 4 cardinal neighbours. Add them to the list if they are valid. If one of them isn't valid, then that direction diagonal is blocked.
                    // Up, Down, Left, Right
                    TryConnectCardinalNeighbour(0, 1, currentPosition, tileMapPosition);
                    TryConnectCardinalNeighbour(0, -1, currentPosition, tileMapPosition);
                    TryConnectCardinalNeighbour(-1, 0, currentPosition, tileMapPosition);
                    TryConnectCardinalNeighbour(1, 0, currentPosition, tileMapPosition);

                    // Top Left, Top Right, Bottom Left, Bottom Right
                    TryConnectDiagonalNeighbour(-1, +1, currentPosition, Direction.TopLeft, tileMapPosition);
                    TryConnectDiagonalNeighbour(+1, +1, currentPosition, Direction.TopRight, tileMapPosition);
                    TryConnectDiagonalNeighbour(-1, -1, currentPosition, Direction.BottomLeft, tileMapPosition);
                    TryConnectDiagonalNeighbour(+1, -1, currentPosition, Direction.BottomRight, tileMapPosition);

                    if (!AlgorithmsUtils.DoInstantPass() && DungeonDataGenerator.Instance.GenerationSettings.delaySettings.GraphCreation != DelayType.Instant)
                        yield return StartCoroutine(AlgorithmsUtils.Delay(DungeonDataGenerator.Instance.GenerationSettings.delaySettings.GraphCreation));
                }
            }
            graphGenerated = true;
        }

        /// <summary>
        /// Checks if the diagonal neighbour is valid. If so, add it to the graph.
        /// </summary>
        /// <param name="neighbourDifferenceX"></param>
        /// <param name="neighbourDifferenceY"></param>
        /// <param name="currentPosition"></param>
        /// <param name="direction"></param>
        private void TryConnectDiagonalNeighbour(int neighbourDifferenceX, int neighbourDifferenceY, Vector3 currentPosition, Direction direction, Vector2Int tileMapPosition)
        {
            Vector3 neighbourPosition = new(neighbourDifferenceX + currentPosition.x, 0, neighbourDifferenceY + currentPosition.z);
            if (tileMap[tileMapPosition.x + neighbourDifferenceX, tileMapPosition.x + neighbourDifferenceY] != 0)
                return;

            if (neighbourPosition.x < dungeonBounds.xMin && (neighbourPosition.x >= dungeonBounds.xMax ||
                neighbourPosition.y < dungeonBounds.yMin) && neighbourPosition.y >= dungeonBounds.yMax)
                return;

            if (CheckCardinals(neighbourPosition, currentPosition, direction))
            {
                Graph.AddNode(currentPosition);
                Graph.AddNode(neighbourPosition);
                Graph.AddEdge(currentPosition, neighbourPosition);
            }
            else
            {
                Debug.Log($"Diagonal {direction} is blocked by cardinal neighbours.");
                DebugExtension.DebugWireSphere(neighbourPosition, Color.red, .2f, 1);
                Debug.DrawLine(currentPosition, neighbourPosition, Color.red, 1);
            }
        }

        /// <summary>
        /// Checks if the cardinal neighbours are connected to the original node to see if the diagonal is allowed.
        /// </summary>
        /// <param name="checkingDiagonal"></param>
        /// <param name="currentPosition"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        private bool CheckCardinals(Vector2 checkingDiagonal, Vector3 currentPosition, Direction direction)
        {
            switch (direction)
            {
                case Direction.TopLeft:
                    return
                        Graph.GetNeighbours(currentPosition).Contains(new Vector3(checkingDiagonal.x + 1, 0, checkingDiagonal.y)) &&
                        Graph.GetNeighbours(currentPosition).Contains(new Vector3(checkingDiagonal.x, 0, checkingDiagonal.y - 1));
                case Direction.TopRight:
                    return
                        Graph.GetNeighbours(currentPosition).Contains(new Vector3(checkingDiagonal.x - 1, 0, checkingDiagonal.y)) &&
                        Graph.GetNeighbours(currentPosition).Contains(new Vector3(checkingDiagonal.x, 0, checkingDiagonal.y - 1));
                case Direction.BottomLeft:
                    return
                        Graph.GetNeighbours(currentPosition).Contains(new Vector3(checkingDiagonal.x + 1, 0, checkingDiagonal.y)) &&
                        Graph.GetNeighbours(currentPosition).Contains(new Vector3(checkingDiagonal.x, 0, checkingDiagonal.y + 1));
                case Direction.BottomRight:
                    return
                        Graph.GetNeighbours(currentPosition).Contains(new Vector3(checkingDiagonal.x - 1, 0, checkingDiagonal.y)) &&
                        Graph.GetNeighbours(currentPosition).Contains(new Vector3(checkingDiagonal.x, 0, checkingDiagonal.y + 1));
                default:
                    Debug.LogError($"Invalid direction: {direction}");
                    return false;
            }
        }

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
            //if (!graphGenerated) return;

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

    }

    internal enum Direction
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
}
