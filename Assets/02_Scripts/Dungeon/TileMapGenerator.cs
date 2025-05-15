using Dungeon.DataStructures;
using NaughtyAttributes;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace Dungeon.Data
{
    /// <summary>
    /// Generates a tile map based on the dungeon generator's data.
    /// </summary>
    [RequireComponent(typeof(DungeonDataGenerator))]
    public class TileMapGenerator : MonoBehaviour
    {
        [SerializeField] private DungeonDataGenerator dungeonGenerator;
        static public int[,] TileMap { get; private set; }

        [Header("Generation Settings")]
        [SerializeField] private Direction arrayFlippingDirections;

        [Header("Events")]
        [SerializeField] private UnityEvent onTileMapGenerated;

        /// <summary>
        /// Generates a tile map based on the dungeon generator's data.
        /// </summary>
        [Button]
        public void GenerateTileMap()
        {
            int[,] _tileMap = new int[dungeonGenerator.DungeonSize.x, dungeonGenerator.DungeonSize.y];

            /*  TODO: generate a tile map.
                Method: Get every rooms data, and then fill it into the tile map. Door data overrides it after filling in doors with a "|" or "-".
            */

            BlankFill(_tileMap);
            GenerateRooms(_tileMap);
            GenerateDoors(_tileMap);

            TileMap = FlipArray(_tileMap, arrayFlippingDirections.HasFlag(Direction.Vertical), arrayFlippingDirections.HasFlag(Direction.Horizontal));

            onTileMapGenerated?.Invoke();
        }

        /// <summary>  
        /// Fills the tile map with empty spaces.  
        /// </summary>  
        /// <param name="_tileMap">The 2D array representing the tile map to be filled with empty spaces.</param>  
        void BlankFill(int[,] _tileMap)
        {
            int _rows = _tileMap.GetLength(0);
            int _cols = _tileMap.GetLength(1);

            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _cols; j++)
                {
                    _tileMap[i, j] = -1; // Empty space  
                }
            }
        }

        /// <summary>  
        /// Populates the tile map with rooms based on the dungeon generator's room data.  
        /// </summary>  
        /// <param name="tileMap">The 2D array representing the tile map to be populated with rooms.</param>  
        void GenerateRooms(int[,] tileMap)
        {
            foreach (Room room in dungeonGenerator.ToDrawRooms)
            {
                RectInt roomSize = room.roomDimensions;
                for (int i = roomSize.x; i < roomSize.x + roomSize.width; i++)
                {
                    for (int j = roomSize.y; j < roomSize.y + roomSize.height; j++)
                    {
                        if (i == roomSize.xMin || i == roomSize.xMax - 1 || j == roomSize.yMin || j == roomSize.yMax - 1)
                            tileMap[i, j] = 1; // Wall  
                        else
                            tileMap[i, j] = 0; // Floor  
                    }
                }
            }
        }

        /// <summary>  
        /// Adds doors to the tile map based on the dungeon generator's door data.  
        /// </summary>  
        /// <param name="tileMap">The 2D array representing the tile map to be updated with door data.</param>  
        void GenerateDoors(int[,] tileMap)
        {
            foreach (RectInt door in dungeonGenerator.Doors)
            {
                for (int i = door.x; i < door.x + door.width; i++)
                {
                    for (int j = door.y; j < door.y + door.height; j++)
                    {
                        //tileMap[i, j] = door.height < door.width
                        //    ? 2  // Horizontal door  
                        //    : 3; // Vertical door
                        tileMap[i, j] = 2;
                    }
                }
            }
        }

        int[,] FlipArray(int[,] _tileMap, bool flipVert = false, bool flipHor = false)
        {
            int rows = _tileMap.GetLength(0);
            int cols = _tileMap.GetLength(1);

            int[,] flippedTileMap = new int[rows, cols];

            for (int j = 0; j != cols; j++)
            {
                for (int i = 0; i != rows; i++)
                {
                    int flippedI = flipVert ? rows - 1 - i : i;
                    int flippedJ = flipHor ? cols - 1 - j : j;

                    flippedTileMap[flippedI, flippedJ] = _tileMap[i, j];
                }
            }

            return flippedTileMap;
        }

        /// <summary>
        /// Converts the generated tile map into a string representation.
        /// </summary>
        /// <param name="flipVert">If true, flips the tile map vertically.</param>
        /// <param name="flipHor">If true, flips the tile map horizontally.</param>
        /// <returns>A string representation of the tile map, where different symbols represent different tile types.</returns>
        public string ToString(bool flipVert = false, bool flipHor = true)
        {
            if (TileMap == null) return "Tile map not generated yet.";

            int rows = TileMap.GetLength(0);
            int cols = TileMap.GetLength(1);

            var sb = new StringBuilder();

            for (int j = 0; j != cols; j++)
            {
                for (int i = 0; i != rows; i++)
                {
                    switch (TileMap[i, j])
                    {
                        case (0):
                            sb.Append("0");
                            break;
                        case (1):
                            sb.Append("#");
                            break;
                        case (2):
                            sb.Append("-");
                            break;
                        case (3):
                            sb.Append("|");
                            break;
                        default:
                            sb.Append(" ");
                            break;
                    }
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        [Button]
        public void PrintTileMap()
        {
            Debug.Log(ToString());
        }
    }
}