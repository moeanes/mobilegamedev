using System.Collections.Generic;
using UnityEngine;

// Furnishes each room by its type, mixing laboratory equipment (lab_stuff.png, the same
// art set as the floor/walls) with medical furniture (hospital_tiles.png) so the facility
// reads as a mixed lab + hospital. Each room gets one centrepiece against the wall opposite
// its door plus two accent pieces in the back corners of the wider rooms.
//
// One piece of furniture per room, flush against the wall opposite the door. Pieces are
// solid (the player and enemies cannot walk through them); enemies steer around them.
// Kept to one per room so the now-larger rooms stay easy to move through.
public static class MapDecorator
{
    private const string LabSheetResource = "Tiles/lab_stuff";
    private const string HospitalSheetResource = "Props/hospital_tiles";
    private const int Tile = 32;

    // Rect(x, y, width, height) in sheet pixels, bottom-left origin. lab_stuff is 1184x512.
    // The big hazard consoles were dropped — they clashed with the doors and read poorly on
    // the floor; rooms use the tidier equipment below.
    private static readonly Dictionary<string, Rect> LabRects = new Dictionary<string, Rect>
    {
        { "tank", new Rect(960f, 192f, 64f, 96f) },
        { "locker", new Rect(1056f, 0f, 128f, 96f) },
        { "shelf", new Rect(1056f, 96f, 128f, 96f) },
        { "machine", new Rect(32f, 416f, 64f, 96f) },
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
            if (room.Bounds.Contains(map.PlayerSpawnCell))
            {
                continue; // leave the room the player spawns in clear of furniture
            }

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
        string piece = FurnitureFor(room.Type)[0];
        int centerColumn = bounds.xMin + bounds.width / 2;
        int centerRow = bounds.yMin + bounds.height / 2;

        // Doorways sit at the MIDDLE of every wall, so a single piece centred on a wall that
        // has NO door can never block a doorway. Prefer the bottom/top wall, then the sides;
        // a fully interior room (a door on all four walls) gets the piece in its centre. A
        // wall has a door when its middle cell is floor (the opening) instead of solid.
        if (!map.IsFloor(new Vector2Int(centerColumn, bounds.yMin - 1)))
        {
            Place(map, parent, sprites, piece, centerColumn, bounds.yMin + 1);
        }
        else if (!map.IsFloor(new Vector2Int(centerColumn, bounds.yMax)))
        {
            Place(map, parent, sprites, piece, centerColumn, bounds.yMax - 2);
        }
        else if (!map.IsFloor(new Vector2Int(bounds.xMin - 1, centerRow)))
        {
            Place(map, parent, sprites, piece, bounds.xMin + 1, centerRow);
        }
        else if (!map.IsFloor(new Vector2Int(bounds.xMax, centerRow)))
        {
            Place(map, parent, sprites, piece, bounds.xMax - 2, centerRow);
        }
        else
        {
            Place(map, parent, sprites, piece, centerColumn, centerRow);
        }
    }

    private static string[] FurnitureFor(RoomType type)
    {
        switch (type)
        {
            case RoomType.Lab: return new[] { "shelf", "tank", "machine" };
            case RoomType.Control: return new[] { "locker", "machine", "tank" };
            case RoomType.Reactor: return new[] { "shelf", "tank", "tank" };
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
            prop.layer = GameLayers.Prop;

            SpriteRenderer renderer = prop.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = -5; // above the floor (-10), below characters (10+)

            // Solid: blocks the player and enemies (so nothing walks through furniture).
            // Enemies steer around it (EnemyNavigation treats Prop as an obstacle) so they
            // never snag on it. Bullets pass over it (only walls stop bullets).
            BoxCollider2D collider = prop.AddComponent<BoxCollider2D>();
            FitColliderToArt(collider, renderer.sprite);
        }
    }

    // Sizes the collision box to the furniture's actual (non-transparent) pixels instead of
    // the full sprite rect, which carries transparent padding. With the padded rect the box
    // is either too small (characters slide onto the art) or too big (an invisible wall in
    // the empty space beside the art). Scanned once at build time per prop; the sheets are
    // marked readable in their import settings.
    private static void FitColliderToArt(BoxCollider2D collider, Sprite sprite)
    {
        Rect rect = sprite.rect;
        int width = (int)rect.width;
        int height = (int)rect.height;
        Color[] pixels = sprite.texture.GetPixels((int)rect.x, (int)rect.y, width, height);

        int minX = width, minY = height, maxX = -1, maxY = -1;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (pixels[y * width + x].a <= 0.1f)
                {
                    continue;
                }

                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }
        }

        if (maxX < minX)
        {
            return; // fully transparent — keep the collider's default size
        }

        collider.size = new Vector2((maxX - minX + 1) / (float)Tile, (maxY - minY + 1) / (float)Tile);
        collider.offset = new Vector2(
            (minX + maxX + 1 - width) * 0.5f / Tile,
            (minY + maxY + 1 - height) * 0.5f / Tile);
    }
}
