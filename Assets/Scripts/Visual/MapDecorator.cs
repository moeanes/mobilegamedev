using System.Collections.Generic;
using UnityEngine;

// Furnishes each room according to its type, cutting hospital furniture from
// hospital_tiles.png (rects are sheet pixel coords, bottom-left origin; sheet 512x336).
// Furniture lines the wall opposite the corridor door (so the entrance stays clear),
// with a few accent pieces along the near wall. Props are solid for the player; enemies
// pass through them.
public static class MapDecorator
{
    private const string SheetResource = "Props/hospital_tiles";
    private const int Tile = 32;

    private static readonly Dictionary<string, Rect> PropRects = new Dictionary<string, Rect>
    {
        { "bed", new Rect(224f, 240f, 72f, 82f) },
        { "bed2", new Rect(288f, 158f, 66f, 84f) },
        { "cabinet", new Rect(4f, 256f, 60f, 64f) },
        { "shelf", new Rect(156f, 254f, 38f, 68f) },
        { "ivstand", new Rect(158f, 242f, 34f, 92f) },
        { "bench", new Rect(6f, 222f, 128f, 30f) },
        { "desk", new Rect(86f, 172f, 64f, 46f) },
        { "monitor", new Rect(158f, 208f, 40f, 34f) },
        { "chair", new Rect(350f, 174f, 34f, 36f) },
        { "plant", new Rect(2f, 130f, 58f, 64f) },
        { "potted", new Rect(2f, 82f, 58f, 64f) },
        { "flowers", new Rect(58f, 94f, 40f, 52f) },
    };

    public static void Decorate(RoomMap map)
    {
        Texture2D sheet = Resources.Load<Texture2D>(SheetResource);
        if (sheet == null)
        {
            return;
        }

        var sprites = new Dictionary<string, Sprite>(PropRects.Count);
        foreach (KeyValuePair<string, Rect> entry in PropRects)
        {
            sprites[entry.Key] = Sprite.Create(sheet, entry.Value, new Vector2(0.5f, 0.5f), Tile);
        }

        Transform parent = new GameObject("Props").transform;
        foreach (Room room in map.Rooms)
        {
            FurnishRoom(map, parent, sprites, room);
        }
    }

    private static void FurnishRoom(RoomMap map, Transform parent, Dictionary<string, Sprite> sprites, Room room)
    {
        RectInt bounds = room.Bounds;
        bool doorOnTop = bounds.yMax <= map.HeightCells / 2;
        int backRow = doorOnTop ? bounds.yMin + 1 : bounds.yMax - 2;
        int frontRow = doorOnTop ? bounds.yMax - 2 : bounds.yMin + 1;

        // Line the far wall with the room's main furniture, packed by each piece's width.
        int x = bounds.xMin + 1;
        foreach (string name in BackWall(room.Type))
        {
            if (x > bounds.xMax - 2)
            {
                break;
            }

            PlaceProp(map, parent, sprites[name], new Vector2Int(x, backRow));
            x += CellWidth(name);
        }

        // Equipment sitting on a lab counter, one row in from the wall.
        x = bounds.xMin + 1;
        foreach (string name in CounterTop(room.Type))
        {
            if (x > bounds.xMax - 2)
            {
                break;
            }

            int counterRow = doorOnTop ? backRow - 1 : backRow + 1;
            PlaceProp(map, parent, sprites[name], new Vector2Int(x, counterRow));
            x += CellWidth(name) + 1;
        }

        // Accent pieces (plants, chairs) spread along the near wall.
        string[] accents = Accents(room.Type);
        int slot = 0;
        for (int ax = bounds.xMin + 1; ax < bounds.xMax - 1 && slot < accents.Length; ax += 3, slot++)
        {
            PlaceProp(map, parent, sprites[accents[slot]], new Vector2Int(ax, frontRow));
        }
    }

    private static string[] BackWall(RoomType type)
    {
        switch (type)
        {
            case RoomType.Ward: return new[] { "bed", "ivstand", "bed", "ivstand", "bed", "ivstand" };
            case RoomType.Lab: return new[] { "bench", "bench", "bench", "bench" };
            case RoomType.Office: return new[] { "desk", "chair", "shelf", "monitor", "cabinet" };
            case RoomType.Pharmacy: return new[] { "cabinet", "shelf", "cabinet", "shelf", "cabinet", "shelf" };
            case RoomType.Waiting: return new[] { "bench", "bench", "bench" };
            default: return System.Array.Empty<string>();
        }
    }

    private static string[] CounterTop(RoomType type)
        => type == RoomType.Lab ? new[] { "monitor", "shelf", "desk" } : System.Array.Empty<string>();

    private static string[] Accents(RoomType type)
    {
        switch (type)
        {
            case RoomType.Ward: return new[] { "cabinet", "potted" };
            case RoomType.Lab: return new[] { "chair", "chair", "plant" };
            case RoomType.Office: return new[] { "plant", "potted", "flowers" };
            case RoomType.Pharmacy: return new[] { "plant" };
            case RoomType.Waiting: return new[] { "potted", "plant", "flowers", "chair" };
            default: return System.Array.Empty<string>();
        }
    }

    private static int CellWidth(string name) => Mathf.Max(1, Mathf.CeilToInt(PropRects[name].width / Tile));

    private static void PlaceProp(RoomMap map, Transform parent, Sprite sprite, Vector2Int cell)
    {
        Vector2 position = map.CellToWorld(cell);

        GameObject prop = new GameObject("Prop");
        prop.transform.SetParent(parent, false);
        prop.transform.position = new Vector3(position.x, position.y, 1f);
        prop.layer = GameLayers.Prop;

        SpriteRenderer renderer = prop.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = -5; // above the floor (-10), below characters (10+)

        BoxCollider2D collider = prop.AddComponent<BoxCollider2D>();
        collider.size = renderer.sprite.bounds.size * 0.7f;
    }
}
