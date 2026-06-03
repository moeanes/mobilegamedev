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

        foreach (RectInt rect in map.FloorRects)
        {
            Vector2 center = map.RectCenterWorld(rect);

            GameObject piece = new GameObject("FloorPiece");
            piece.transform.SetParent(parent, false);
            piece.transform.position = new Vector3(center.x, center.y, 5f);

            SpriteRenderer renderer = piece.AddComponent<SpriteRenderer>();
            renderer.sprite = tile;
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.size = new Vector2(rect.width, rect.height);
            renderer.sortingOrder = -10;
        }
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

    // Cuts one 32x32 tile out of a sheet. Sheets are addressed from the top row down;
    // Unity's Rect is bottom-left based, hence the height flip.
    private static Sprite Slice(Texture2D sheet, int column, int rowFromTop)
    {
        int x = column * Tile;
        int y = sheet.height - (rowFromTop + 1) * Tile;
        return Sprite.Create(sheet, new Rect(x, y, Tile, Tile), new Vector2(0.5f, 0.5f), Tile, 0, SpriteMeshType.FullRect);
    }
}
