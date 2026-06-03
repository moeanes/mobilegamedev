using System.Collections.Generic;
using UnityEngine;

// Furnishes each room by its type, mixing laboratory equipment (lab_stuff.png, the same
// art set as the floor/walls) with medical furniture (hospital_tiles.png) so the facility
// reads as a mixed lab + hospital. Each room gets one centrepiece against the wall opposite
// its door plus two accent pieces in the back corners of the wider rooms.
//
// Furniture is DECORATION ONLY — no colliders — so the player can never get stuck on it
// (only the walls block movement). Pieces sit behind the characters.
public static class MapDecorator
{
    private const string LabSheetResource = "Tiles/lab_stuff";
    private const string HospitalSheetResource = "Props/hospital_tiles";
    private const int Tile = 32;
    private const int AccentMinWidth = 11;

    // Rect(x, y, width, height) in sheet pixels, bottom-left origin. lab_stuff is 1184x512.
    private static readonly Dictionary<string, Rect> LabRects = new Dictionary<string, Rect>
    {
        { "console_blue", new Rect(64f, 288f, 192f, 96f) },
        { "console_red", new Rect(288f, 96f, 192f, 96f) },
        { "tank", new Rect(960f, 192f, 64f, 96f) },
        { "locker", new Rect(1056f, 0f, 128f, 96f) },
        { "shelf", new Rect(1056f, 96f, 128f, 96f) },
        { "machine", new Rect(32f, 416f, 64f, 96f) },
        { "hazard", new Rect(640f, 32f, 64f, 160f) },
    };

    // hospital_tiles is 512x336.
    private static readonly Dictionary<string, Rect> HospitalRects = new Dictionary<string, Rect>
    {
        { "bed", new Rect(224f, 240f, 72f, 82f) },
        { "ivstand", new Rect(158f, 242f, 34f, 92f) },
        { "cabinet", new Rect(4f, 256f, 60f, 64f) },
        { "desk", new Rect(86f, 172f, 64f, 46f) },
        { "chair", new Rect(350f, 174f, 34f, 36f) },
        { "plant", new Rect(2f, 130f, 58f, 64f) },
    };

    public static void Decorate(RoomMap map)
    {
        var sprites = new Dictionary<string, Sprite>();
        LoadSprites(LabSheetResource, LabRects, sprites);
        LoadSprites(HospitalSheetResource, HospitalRects, sprites);

        Transform parent = new GameObject("Props").transform;
        foreach (Room room in map.Rooms)
        {
            FurnishRoom(map, parent, sprites, room);
        }
    }

    private static void LoadSprites(string resource, Dictionary<string, Rect> rects, Dictionary<string, Sprite> into)
    {
        Texture2D sheet = Resources.Load<Texture2D>(resource);
        if (sheet == null)
        {
            return;
        }

        foreach (KeyValuePair<string, Rect> entry in rects)
        {
            into[entry.Key] = Sprite.Create(sheet, entry.Value, new Vector2(0.5f, 0.5f), Tile);
        }
    }

    private static void FurnishRoom(RoomMap map, Transform parent, Dictionary<string, Sprite> sprites, Room room)
    {
        RectInt bounds = room.Bounds;
        string[] plan = FurnitureFor(room.Type); // [centrepiece, leftAccent, rightAccent]

        bool doorOnTop = bounds.yMax <= map.HeightCells / 2;
        int backRow = doorOnTop ? bounds.yMin + 1 : bounds.yMax - 2;

        Place(map, parent, sprites, plan[0], bounds.xMin + bounds.width / 2, backRow);

        if (bounds.width >= AccentMinWidth)
        {
            Place(map, parent, sprites, plan[1], bounds.xMin + 1, backRow);
            Place(map, parent, sprites, plan[2], bounds.xMax - 2, backRow);
        }
    }

    private static string[] FurnitureFor(RoomType type)
    {
        switch (type)
        {
            case RoomType.Lab: return new[] { "console_blue", "tank", "machine" };
            case RoomType.Control: return new[] { "console_red", "machine", "tank" };
            case RoomType.Reactor: return new[] { "console_blue", "hazard", "hazard" };
            case RoomType.Storage: return new[] { "locker", "shelf", "shelf" };
            case RoomType.Ward: return new[] { "bed", "ivstand", "cabinet" };
            case RoomType.MedBay: return new[] { "bed", "bed", "cabinet" };
            case RoomType.Office: return new[] { "desk", "chair", "plant" };
            default: return new[] { "machine", "tank", "tank" };
        }
    }

    private static void Place(RoomMap map, Transform parent, Dictionary<string, Sprite> sprites, string name, int column, int row)
    {
        if (sprites.TryGetValue(name, out Sprite sprite))
        {
            Vector2 position = map.CellToWorld(new Vector2Int(column, row));

            GameObject prop = new GameObject("Prop");
            prop.transform.SetParent(parent, false);
            prop.transform.position = new Vector3(position.x, position.y, 1f);

            SpriteRenderer renderer = prop.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = -5; // above the floor (-10), below characters (10+)
            // No collider: furniture is decoration, so the player never snags on it.
        }
    }
}
