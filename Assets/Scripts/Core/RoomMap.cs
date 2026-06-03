using System.Collections.Generic;
using UnityEngine;

public enum RoomType { Office, Lab, Ward, Pharmacy, Waiting }

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

// A hand-designed hospital floor: one central corridor with themed rooms opening off it
// (doctor's office, laboratory, patient wards, pharmacy, waiting room). This is plain
// data with no scene objects; MapBuilder draws the floor + walls and MapDecorator
// furnishes each room according to its type.
//
// Coordinates are bottom-left based cells, one cell = one world unit. To redesign the
// floor, edit Hallway + RoomDefs + DoorRects + SpawnCell below.
public class RoomMap
{
    private const int Width = 38;
    private const int Height = 21;

    private static readonly RectInt Hallway = new RectInt(1, 9, 36, 3);

    private static readonly Room[] RoomDefs =
    {
        new Room(new RectInt(1, 1, 11, 7), RoomType.Waiting),    // bottom-left
        new Room(new RectInt(13, 1, 11, 7), RoomType.Pharmacy),  // bottom-mid
        new Room(new RectInt(25, 1, 12, 7), RoomType.Ward),      // bottom-right
        new Room(new RectInt(1, 13, 13, 7), RoomType.Office),    // top-left
        new Room(new RectInt(15, 13, 10, 7), RoomType.Lab),      // top-mid
        new Room(new RectInt(26, 13, 11, 7), RoomType.Ward),     // top-right
    };

    // Two-cell openings linking each room to the corridor.
    private static readonly RectInt[] DoorRects =
    {
        new RectInt(5, 8, 2, 1), new RectInt(17, 8, 2, 1), new RectInt(30, 8, 2, 1),
        new RectInt(6, 12, 2, 1), new RectInt(19, 12, 2, 1), new RectInt(30, 12, 2, 1),
    };

    private static readonly Vector2Int SpawnCell = new Vector2Int(19, 10);

    public int WidthCells => Width;
    public int HeightCells => Height;
    public Vector2 WorldMin { get; }
    public Vector2 WorldMax { get; }
    public Vector2 PlayerSpawn { get; }

    public IReadOnlyList<Room> Rooms => rooms;
    public IReadOnlyList<RectInt> FloorRects => floorRects;
    public IReadOnlyList<Vector2> FloorPoints => floorPoints;

    private readonly List<Room> rooms = new List<Room>();
    private readonly List<RectInt> floorRects = new List<RectInt>();
    private readonly HashSet<Vector2Int> floorCells = new HashSet<Vector2Int>();
    private readonly List<Vector2> floorPoints = new List<Vector2>();
    private readonly Vector2 originOffset;

    // levelIndex is accepted for the call site and reserved for future per-level
    // variants; the current design is the same handcrafted floor for every level.
    public RoomMap(int levelIndex)
    {
        originOffset = new Vector2(-Width * 0.5f, -Height * 0.5f);
        WorldMin = originOffset;
        WorldMax = originOffset + new Vector2(Width, Height);

        AddFloorRect(Hallway);

        foreach (Room room in RoomDefs)
        {
            rooms.Add(room);
            AddFloorRect(room.Bounds);
        }

        foreach (RectInt door in DoorRects)
        {
            AddFloorRect(door);
        }

        PlayerSpawn = CellToWorld(SpawnCell);

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

    // Every cell inside the map that isn't floor is a wall, so the rooms read as carved
    // out of one solid block. MapBuilder merges these into strips and adds colliders.
    public HashSet<Vector2Int> WallCells()
    {
        var walls = new HashSet<Vector2Int>();
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
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
}
