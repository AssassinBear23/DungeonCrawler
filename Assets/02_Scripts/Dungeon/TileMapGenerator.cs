using Dungeon.DataStructures;
using NaughtyAttributes;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace Dungeon.Generation
{
    [RequireComponent(typeof(DungeonGenerator))]
    public class TileMapGenerator : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent onGenerateTileMap;
        [SerializeField]
        private DungeonGenerator dungeonGenerator;
        [Space(10)]
        private int[,] _tileMap;
        [Button]
        public void GenerateTileMap()
        {
            int[,] tileMap = new int[dungeonGenerator.DungeonSize.x, dungeonGenerator.DungeonSize.y];
            int _rows = tileMap.GetLength(0);
            int _cols = tileMap.GetLength(1);

            /*  TODO: generate a tilemap.
                Method: Get every rooms data, and then fill it into the tilemap. Door data overrides it after filling in doors with a "\".
            */

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

            foreach (RectInt door in dungeonGenerator.Doors)
            {
                for (int i = door.x; i < door.x + door.width; i++)
                {
                    for (int j = door.y; j < door.y + door.height; j++)
                    {
                        tileMap[i, j] = 2; // Door
                    }
                }
            }


            _tileMap = tileMap;

            onGenerateTileMap?.Invoke();
        }

        public string ToString(bool flipVert = false, bool flipHor = true)
        {
            if (_tileMap == null) return "Tile map not generated yet.";

            int rows = _tileMap.GetLength(0);
            int cols = _tileMap.GetLength(1);

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
                    switch (_tileMap[i, j])
                    {
                        case (0):
                            sb.Append("0");
                            break;
                        case (1):
                            sb.Append("#");
                            break;
                        case (2):
                            sb.Append('%');
                            break;
                        default:
                            sb.Append("E");
                            break;
                    }
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public int[,] GetTileMap()
        {
            return _tileMap;
        }

        [Button]
        public void PrintTileMap()
        {
            Debug.Log(ToString());
        }
    }
}