using System.Collections.Generic;
using UnityEngine;

public enum RoomType { Lab, Control, Reactor, Storage, Ward, MedBay, Office }

public readonly struct Room
{
    public readonly RectInt Bounds;
    public readonly RoomType Type;

    public Room(RectInt bounds, RoomType type)
    {
        Bounds = bounds;
        Type = type;
    }
}

// Hand-designed maps, one per level (1-5), each a different arrangement of rooms joined
// by wide doorways/corridors so the player never gets boxed in. Rooms mix laboratory and
// medical types; MapDecorator furnishes them. This is plain data — MapBuilder draws the
// floor + walls and LevelManager uses the floor for enemy spawns.
//
// Coordinates are bottom-left based cells, one cell = one world unit. To edit a level,
// change its layout in Layouts below (rooms + corridors + spawn). Doorways are kept >=2
// cells wide on purpose.
public class RoomMap
{
    private readonly struct Layout
    {
        public readonly int Width;
        public readonly int Height;
        public readonly Room[] Rooms;
        public readonly RectInt[] Corridors; // halls + door openings: floor, not furnished
        public readonly Vector2Int Spawn;

        public Layout(int width, int height, Room[] rooms, RectInt[] corridors, Vector2Int spawn)
        {
            Width = width;
            Height = height;
            Rooms = rooms;
            Corridors = corridors;
            Spawn = spawn;
        }
    }

    // Each level is a full building floor plan: a grid of rooms with thin shared walls
    // and 3-wide doorways, getting denser as levels progress. The grid fills the map so
    // there are no wasted empty areas.
    private static readonly Layout[] Layouts =
    {
        BuildGrid(3, 2, 10, 7), // L1 — six rooms
        BuildGrid(3, 3, 9, 7),  // L2 — nine rooms
        BuildGrid(4, 3, 9, 6),  // L3 — twelve rooms
        BuildGrid(4, 3, 10, 7), // L4 — twelve bigger rooms
        BuildGrid(4, 4, 9, 6),  // L5 — sixteen rooms, the final floor
    };

    private readonly int width;
    private readonly int height;
    private readonly List<Room> rooms = new List<Room>();
    private readonly List<RectInt> floorRects = new List<RectInt>();
    private readonly List<RectInt> doors = new List<RectInt>();
    private readonly HashSet<Vector2Int> floorCells = new HashSet<Vector2Int>();
    private readonly List<Vector2> floorPoints = new List<Vector2>();
    private readonly Vector2 originOffset;

    public int WidthCells => width;
    public int HeightCells => height;
    public Vector2 WorldMin { get; }
    public Vector2 WorldMax { get; }
    public Vector2 PlayerSpawn { get; }
    public Vector2Int PlayerSpawnCell { get; }

    public IReadOnlyList<Room> Rooms => rooms;
    public IReadOnlyList<RectInt> FloorRects => floorRects;
    public IReadOnlyList<RectInt> Doors => doors;
    public IReadOnlyList<Vector2> FloorPoints => floorPoints;

    public RoomMap(int levelIndex)
    {
        Layout layout = Layouts[Mathf.Clamp(levelIndex - 1, 0, Layouts.Length - 1)];
        width = layout.Width;
        height = layout.Height;

        originOffset = new Vector2(-width * 0.5f, -height * 0.5f);
        WorldMin = originOffset;
        WorldMax = originOffset + new Vector2(width, height);

        foreach (Room room in layout.Rooms)
        {
            rooms.Add(room);
            AddFloorRect(room.Bounds);
        }

        foreach (RectInt corridor in layout.Corridors)
        {
            AddFloorRect(corridor);
            doors.Add(corridor);
        }

        PlayerSpawnCell = layout.Spawn;
        PlayerSpawn = CellToWorld(layout.Spawn);

        foreach (Vector2Int cell in floorCells)
        {
            floorPoints.Add(CellToWorld(cell));
        }
    }

    public bool IsFloor(Vector2Int cell) => floorCells.Contains(cell);

    public Vector2 CellToWorld(Vector2Int cell)
        => new Vector2(cell.x + 0.5f, cell.y + 0.5f) + originOffset;

    public Vector2 RectCenterWorld(RectInt rect)
        => new Vector2(rect.x + rect.width * 0.5f, rect.y + rect.height * 0.5f) + originOffset;

    // Every cell inside the map that isn't floor is a wall.
    public HashSet<Vector2Int> WallCells()
    {
        var walls = new HashSet<Vector2Int>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                if (!floorCells.Contains(cell))
                {
                    walls.Add(cell);
                }
            }
        }

        return walls;
    }

    private void AddFloorRect(RectInt rect)
    {
        floorRects.Add(rect);
        for (int x = rect.xMin; x < rect.xMax; x++)
        {
            for (int y = rect.yMin; y < rect.yMax; y++)
            {
                floorCells.Add(new Vector2Int(x, y));
            }
        }
    }

    // Builds one floor: a cols x rows grid of rooms, each roomW x roomH, separated by
    // one-cell walls and joined to neighbours by 3-wide doorways. The grid fills the whole
    // map (rooms + thin walls + a one-cell border) so no space is wasted.
    private static Layout BuildGrid(int cols, int rows, int roomWidth, int roomHeight)
    {
        int mapWidth = cols * roomWidth + (cols - 1) + 2;
        int mapHeight = rows * roomHeight + (rows - 1) + 2;

        var roomList = new List<Room>();
        var doorList = new List<RectInt>();
        int index = 0;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                int x = 1 + col * (roomWidth + 1);
                int y = 1 + row * (roomHeight + 1);
                roomList.Add(new Room(new RectInt(x, y, roomWidth, roomHeight), TypeForIndex(index)));
                index++;

                if (col + 1 < cols)
                {
                    doorList.Add(new RectInt(x + roomWidth, y + (roomHeight - 3) / 2, 1, 3));
                }
                if (row + 1 < rows)
                {
                    doorList.Add(new RectInt(x + (roomWidth - 3) / 2, y + roomHeight, 3, 1));
                }
            }
        }

        int spawnX = 1 + (cols / 2) * (roomWidth + 1) + roomWidth / 2;
        int spawnY = 1 + (rows / 2) * (roomHeight + 1) + roomHeight / 2;
        return new Layout(mapWidth, mapHeight, roomList.ToArray(), doorList.ToArray(), new Vector2Int(spawnX, spawnY));
    }

    // Cycles room types so every floor mixes laboratory and medical rooms.
    private static RoomType TypeForIndex(int index)
    {
        RoomType[] cycle = { RoomType.Lab, RoomType.Ward, RoomType.Control, RoomType.MedBay, RoomType.Reactor, RoomType.Office, RoomType.Storage };
        return cycle[index % cycle.Length];
    }
}
