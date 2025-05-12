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
        [field: SerializeField] static public int[,] TileMap { get; private set; }

        [field: Header("Events")]
        [SerializeField] private UnityEvent onTileMapGenerated;

        /// <summary>
        /// Generates a tile map based on the dungeon generator's data.
        /// </summary>
        [Button]
        public void GenerateTileMap()
        {
            int[,]  _tileMap = new int[dungeonGenerator.DungeonSize.x, dungeonGenerator.DungeonSize.y];
            int _rows = _tileMap.GetLength(0);
            int _cols = _tileMap.GetLength(1);

            /*  TODO: generate a tile map.
                Method: Get every rooms data, and then fill it into the tile map. Door data overrides it after filling in doors with a "|" or "-".
            */

            BlankFill(_tileMap, _rows, _cols);
            GenerateRooms(_tileMap);
            GenerateDoors(_tileMap);

            TileMap = _tileMap;

            onTileMapGenerated?.Invoke();

            static void BlankFill(int[,] tileMap, int _rows, int _cols)
            {
                for (int i = 0; i < _rows; i++)
                {
                    for (int j = 0; j < _cols; j++)
                    {
                        tileMap[i, j] = -1; // Empty space
                    }
                }
            }

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

            void GenerateDoors(int[,] tileMap)
            {
                foreach (RectInt door in dungeonGenerator.Doors)
                {

                    for (int i = door.x; i < door.x + door.width; i++)
                    {
                        for (int j = door.y; j < door.y + door.height; j++)
                        {
                            tileMap[i, j] = door.height > door.width
                                ? 2  // Horizontal door
                                : 3; // Vertical door
                        }
                    }
                }
            }
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

            int startVert = flipVert ? rows - 1 : 0;
            int endVert = flipVert ? -1 : rows;
            int stepVert = flipVert ? -1 : 1;

            int startHor = flipHor ? cols - 1 : 0;
            int endHor = flipHor ? -1 : cols;
            int stepHor = flipHor ? -1 : 1;

            for (int i = startVert; i != endVert; i += stepVert)
            {
                for (int j = startHor; j != endHor; j += stepHor)
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