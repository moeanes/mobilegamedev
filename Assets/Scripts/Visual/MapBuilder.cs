using System.Collections.Generic;
using UnityEngine;

// Turns a RoomMap (pure data) into scene objects using the Land-of-Pixels laboratory
// tileset already imported under Resources/Tiles: a tiled blue floor sprite per room or
// corridor rectangle, and grey wall strips (merged per row) that carry the colliders.
// Both tiles are cut straight from the sheets and tile seamlessly.
public static class MapBuilder
{
    private const int Tile = 32;

    // (column, row-from-top) of the tile cut from each sheet. Chosen so they repeat
    // without seams: floor = clean gridded blue, wall = clean grey panel.
    private const int FloorColumn = 8;
    private const int FloorRow = 11;
    private const int WallColumn = 7;
    private const int WallRow = 8;

    // Door pieces in lab_stuff.png (column, row-from-top): hazard stripe panel + glowing core.
    private const int HazardColumn = 21;
    private const int HazardRow = 11;
    private const int CoreColumn = 5;
    private const int CoreRow = 5;

    public static void Build(RoomMap map)
    {
        Texture2D floorSheet = Resources.Load<Texture2D>("Tiles/lab_floor");
        Texture2D wallSheet = Resources.Load<Texture2D>("Tiles/lab_walls");
        if (floorSheet == null || wallSheet == null)
        {
            return;
        }

        Sprite floorTile = Slice(floorSheet, FloorColumn, FloorRow);
        Sprite wallTile = Slice(wallSheet, WallColumn, WallRow);

        BuildFloor(map, floorTile);
        BuildWalls(map, wallTile);

        Texture2D stuffSheet = Resources.Load<Texture2D>("Tiles/lab_stuff");
        if (stuffSheet != null)
        {
            BuildDoors(map, Slice(stuffSheet, HazardColumn, HazardRow), Slice(stuffSheet, CoreColumn, CoreRow));
        }
    }

    // An auto-door over each doorway: a hazard-stripe panel with a glowing core that blocks
    // like a wall but opens when a character approaches (see AutoDoor).
    private static void BuildDoors(RoomMap map, Sprite hazardTile, Sprite coreTile)
    {
        Transform parent = new GameObject("Doors").transform;

        foreach (RectInt door in map.Doors)
        {
            Vector2 center = map.RectCenterWorld(door);

            GameObject obj = new GameObject("Door");
            obj.transform.SetParent(parent, false);
            obj.transform.position = new Vector3(center.x, center.y, 1.5f);
            obj.layer = GameLayers.Wall;

            SpriteRenderer panel = obj.AddComponent<SpriteRenderer>();
            panel.sprite = hazardTile;
            panel.drawMode = SpriteDrawMode.Tiled;
            panel.size = new Vector2(door.width, door.height);
            panel.sortingOrder = -3; // above walls (-4), below characters

            BoxCollider2D collider = obj.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(door.width, door.height);

            GameObject core = new GameObject("Core");
            core.transform.SetParent(obj.transform, false);
            core.transform.localPosition = Vector3.zero;
            SpriteRenderer coreRenderer = core.AddComponent<SpriteRenderer>();
            coreRenderer.sprite = coreTile;
            coreRenderer.sortingOrder = -2;

            obj.AddComponent<AutoDoor>();
        }
    }

    private static void BuildFloor(RoomMap map, Sprite tile)
    {
        Transform parent = new GameObject("Floor").transform;

        // One floor spanning the whole map — under the walls too. When the boss smashes the
        // interior walls open, the floor is already there, so no holes show through.
        Vector2 center = (map.WorldMin + map.WorldMax) * 0.5f;

        GameObject piece = new GameObject("FloorPiece");
        piece.transform.SetParent(parent, false);
        piece.transform.position = new Vector3(center.x, center.y, 5f);

        SpriteRenderer renderer = piece.AddComponent<SpriteRenderer>();
        renderer.sprite = tile;
        renderer.drawMode = SpriteDrawMode.Tiled;
        renderer.size = new Vector2(map.WidthCells, map.HeightCells);
        renderer.sortingOrder = -10;
    }

    private static void BuildWalls(RoomMap map, Sprite tile)
    {
        HashSet<Vector2Int> wallCells = map.WallCells();
        Transform parent = new GameObject("Walls").transform;

        // Merge neighbouring wall cells along each row into one strip, so the level ends
        // up with a few dozen wall objects instead of a few hundred single tiles.
        for (int y = 0; y < map.HeightCells; y++)
        {
            int x = 0;
            while (x < map.WidthCells)
            {
                if (!wallCells.Contains(new Vector2Int(x, y)))
                {
                    x++;
                    continue;
                }

                int start = x;
                while (x < map.WidthCells && wallCells.Contains(new Vector2Int(x, y)))
                {
                    x++;
                }

                MakeWallStrip(map, parent, tile, start, y, x - start);
            }
        }
    }

    private static void MakeWallStrip(RoomMap map, Transform parent, Sprite tile, int startX, int y, int length)
    {
        Vector2 left = map.CellToWorld(new Vector2Int(startX, y));
        Vector3 center = new Vector3(left.x + (length - 1) * 0.5f, left.y, 2f);

        GameObject wall = new GameObject("Wall");
        wall.transform.SetParent(parent, false);
        wall.transform.position = center;
        wall.layer = GameLayers.Wall;

        SpriteRenderer renderer = wall.AddComponent<SpriteRenderer>();
        renderer.sprite = tile;
        renderer.drawMode = SpriteDrawMode.Tiled;
        renderer.size = new Vector2(length, 1f);
        renderer.sortingOrder = -4;

        BoxCollider2D collider = wall.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(length, 1f);
    }

    // Smashes every interior wall and door into one open arena, leaving only the outer
    // border so the player stays in bounds. Called when the boss lands. The full-map floor
    // is already under the removed walls, so nothing shows through.
    public static void OpenArena(RoomMap map)
    {
        if (map == null)
        {
            return;
        }

        Texture2D wallSheet = Resources.Load<Texture2D>("Tiles/lab_walls");
        Sprite wallTile = wallSheet != null ? Slice(wallSheet, WallColumn, WallRow) : null;

        // Spray breaking chunks where the interior walls stand, THEN swap them for the open
        // arena — so the walls look like they shatter rather than vanish.
        if (wallTile != null)
        {
            SpawnCrumble(map, wallTile);
        }

        DestroyByName("Walls");
        DestroyByName("Doors");

        if (wallTile == null)
        {
            return;
        }

        Transform parent = new GameObject("Walls").transform;

        float width = map.WidthCells;
        float height = map.HeightCells;
        Vector2 center = (map.WorldMin + map.WorldMax) * 0.5f;

        MakeWallBox(parent, wallTile, new Vector2(center.x, map.WorldMin.y + 0.5f), new Vector2(width, 1f));
        MakeWallBox(parent, wallTile, new Vector2(center.x, map.WorldMax.y - 0.5f), new Vector2(width, 1f));
        MakeWallBox(parent, wallTile, new Vector2(map.WorldMin.x + 0.5f, center.y), new Vector2(1f, height));
        MakeWallBox(parent, wallTile, new Vector2(map.WorldMax.x - 0.5f, center.y), new Vector2(1f, height));
    }

    // A breaking chunk at every interior wall cell, flung outward from the centre — the
    // visible "smash". Border cells are left out so the outer wall stays whole.
    private static void SpawnCrumble(RoomMap map, Sprite tile)
    {
        Transform parent = new GameObject("WallDebris").transform;
        int width = map.WidthCells;
        int height = map.HeightCells;
        Vector2 center = (map.WorldMin + map.WorldMax) * 0.5f;

        foreach (Vector2Int cell in map.WallCells())
        {
            if (cell.x == 0 || cell.y == 0 || cell.x == width - 1 || cell.y == height - 1)
            {
                continue;
            }

            Vector2 position = map.CellToWorld(cell);

            GameObject chunk = new GameObject("Debris");
            chunk.transform.SetParent(parent, false);
            chunk.transform.position = new Vector3(position.x, position.y, 1f);

            SpriteRenderer renderer = chunk.AddComponent<SpriteRenderer>();
            renderer.sprite = tile;
            renderer.sortingOrder = 8; // over the floor and walls, under the characters

            Vector2 outward = position - center;
            outward = outward.sqrMagnitude > 0.01f ? outward.normalized : Vector2.up;
            Vector2 velocity = outward * Random.Range(1.5f, 4f) + Vector2.up * Random.Range(1f, 3f);

            chunk.AddComponent<WallDebris>().Launch(velocity, Random.Range(-360f, 360f), Random.Range(0.45f, 0.8f));
        }
    }

    private static void DestroyByName(string name)
    {
        GameObject existing = GameObject.Find(name);
        if (existing != null)
        {
            Object.Destroy(existing);
        }
    }

    private static void MakeWallBox(Transform parent, Sprite tile, Vector2 center, Vector2 size)
    {
        GameObject wall = new GameObject("Wall");
        wall.transform.SetParent(parent, false);
        wall.transform.position = new Vector3(center.x, center.y, 2f);
        wall.layer = GameLayers.Wall;

        SpriteRenderer renderer = wall.AddComponent<SpriteRenderer>();
        renderer.sprite = tile;
        renderer.drawMode = SpriteDrawMode.Tiled;
        renderer.size = size;
        renderer.sortingOrder = -4;

        BoxCollider2D collider = wall.AddComponent<BoxCollider2D>();
        collider.size = size;
    }

    // Cuts one 32x32 tile out of a sheet. Sheets are addressed from the top row down;
    // Unity's Rect is bottom-left based, hence the height flip.
    private static Sprite Slice(Texture2D sheet, int column, int rowFromTop)
    {
        int x = column * Tile;
        int y = sheet.height - (rowFromTop + 1) * Tile;
        return Sprite.Create(sheet, new Rect(x, y, Tile, Tile), new Vector2(0.5f, 0.5f), Tile, 0, SpriteMeshType.FullRect);
    }
}
