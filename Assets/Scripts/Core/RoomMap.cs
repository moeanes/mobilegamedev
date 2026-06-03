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

    private static readonly Layout[] Layouts = { BuildLevel1(), BuildLevel2(), BuildLevel3(), BuildLevel4(), BuildLevel5() };

    private readonly int width;
    private readonly int height;
    private readonly List<Room> rooms = new List<Room>();
    private readonly List<RectInt> floorRects = new List<RectInt>();
    private readonly HashSet<Vector2Int> floorCells = new HashSet<Vector2Int>();
    private readonly List<Vector2> floorPoints = new List<Vector2>();
    private readonly Vector2 originOffset;

    public int WidthCells => width;
    public int HeightCells => height;
    public Vector2 WorldMin { get; }
    public Vector2 WorldMax { get; }
    public Vector2 PlayerSpawn { get; }

    public IReadOnlyList<Room> Rooms => rooms;
    public IReadOnlyList<RectInt> FloorRects => floorRects;
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
        }

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

    // Level 1 — four big open rooms (2x2), wide doorways. Gentle introduction.
    private static Layout BuildLevel1()
    {
        Room[] r =
        {
            new Room(new RectInt(2, 2, 10, 6), RoomType.Ward),
            new Room(new RectInt(14, 2, 10, 6), RoomType.Lab),
            new Room(new RectInt(2, 10, 10, 6), RoomType.Control),
            new Room(new RectInt(14, 10, 10, 6), RoomType.Storage),
        };
        RectInt[] c = { new RectInt(12, 3, 2, 4), new RectInt(12, 11, 2, 4), new RectInt(5, 8, 4, 2), new RectInt(17, 8, 4, 2) };
        return new Layout(26, 18, r, c, new Vector2Int(7, 5));
    }

    // Level 2 — central corridor with six rooms opening off it.
    private static Layout BuildLevel2()
    {
        Room[] r =
        {
            new Room(new RectInt(1, 1, 11, 7), RoomType.MedBay),
            new Room(new RectInt(13, 1, 11, 7), RoomType.Storage),
            new Room(new RectInt(25, 1, 12, 7), RoomType.Lab),
            new Room(new RectInt(1, 13, 13, 7), RoomType.Office),
            new Room(new RectInt(15, 13, 10, 7), RoomType.Reactor),
            new Room(new RectInt(26, 13, 11, 7), RoomType.Ward),
        };
        RectInt[] c =
        {
            new RectInt(1, 9, 36, 3),
            new RectInt(5, 8, 2, 1), new RectInt(17, 8, 2, 1), new RectInt(30, 8, 2, 1),
            new RectInt(6, 12, 2, 1), new RectInt(19, 12, 2, 1), new RectInt(30, 12, 2, 1),
        };
        return new Layout(38, 21, r, c, new Vector2Int(19, 10));
    }

    // Level 3 — a central arena with four rooms branching off it.
    private static Layout BuildLevel3()
    {
        Room[] r =
        {
            new Room(new RectInt(11, 9, 9, 7), RoomType.Reactor),
            new Room(new RectInt(11, 18, 9, 4), RoomType.Lab),
            new Room(new RectInt(11, 4, 9, 3), RoomType.MedBay),
            new Room(new RectInt(1, 9, 8, 7), RoomType.Control),
            new Room(new RectInt(22, 9, 8, 7), RoomType.Storage),
        };
        RectInt[] c = { new RectInt(13, 16, 3, 2), new RectInt(13, 7, 3, 2), new RectInt(9, 11, 2, 3), new RectInt(20, 11, 2, 3) };
        return new Layout(31, 23, r, c, new Vector2Int(15, 12));
    }

    // Level 4 — two wings of rooms either side of a big central hall.
    private static Layout BuildLevel4()
    {
        Room[] r =
        {
            new Room(new RectInt(1, 1, 9, 6), RoomType.Lab),
            new Room(new RectInt(1, 9, 9, 6), RoomType.Control),
            new Room(new RectInt(1, 17, 9, 5), RoomType.Storage),
            new Room(new RectInt(25, 1, 9, 6), RoomType.Ward),
            new Room(new RectInt(25, 9, 9, 6), RoomType.MedBay),
            new Room(new RectInt(25, 17, 9, 5), RoomType.Office),
        };
        RectInt[] c =
        {
            new RectInt(12, 1, 11, 21),
            new RectInt(10, 3, 2, 3), new RectInt(10, 11, 2, 3), new RectInt(10, 18, 2, 3),
            new RectInt(23, 3, 2, 3), new RectInt(23, 11, 2, 3), new RectInt(23, 18, 2, 3),
        };
        return new Layout(35, 23, r, c, new Vector2Int(17, 11));
    }

    // Level 5 — a dense 3x3 grid of rooms. The final floor.
    private static Layout BuildLevel5()
    {
        Room[] r =
        {
            new Room(new RectInt(1, 1, 9, 6), RoomType.Lab),
            new Room(new RectInt(13, 1, 9, 6), RoomType.Storage),
            new Room(new RectInt(25, 1, 9, 6), RoomType.Ward),
            new Room(new RectInt(1, 8, 9, 6), RoomType.Control),
            new Room(new RectInt(13, 8, 9, 6), RoomType.Reactor),
            new Room(new RectInt(25, 8, 9, 6), RoomType.MedBay),
            new Room(new RectInt(1, 15, 9, 6), RoomType.Office),
            new Room(new RectInt(13, 15, 9, 6), RoomType.Lab),
            new Room(new RectInt(25, 15, 9, 6), RoomType.Storage),
        };
        RectInt[] c =
        {
            new RectInt(10, 3, 3, 2), new RectInt(22, 3, 3, 2),
            new RectInt(10, 10, 3, 2), new RectInt(22, 10, 3, 2),
            new RectInt(10, 17, 3, 2), new RectInt(22, 17, 3, 2),
            new RectInt(4, 7, 2, 1), new RectInt(16, 7, 2, 1), new RectInt(28, 7, 2, 1),
            new RectInt(4, 14, 2, 1), new RectInt(16, 14, 2, 1), new RectInt(28, 14, 2, 1),
        };
        return new Layout(35, 23, r, c, new Vector2Int(17, 10));
    }
}
