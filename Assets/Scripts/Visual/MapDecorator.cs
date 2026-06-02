using System.Collections.Generic;
using UnityEngine;

// Scatters hospital furniture (cut from hospital_tiles.png) across the floor as
// decoration. The rects are hand-picked in the sheet's pixel coordinates
// (bottom-left origin); tweak them if a prop looks clipped.
public static class MapDecorator
{
    private const string SheetResource = "Props/hospital_tiles";

    // Rect(x, y, width, height) in texture pixels. y is measured from the BOTTOM.
    // hospital_tiles.png is 512x336.
    private static readonly Rect[] PropRects =
    {
        new Rect(228f, 256f, 66f, 70f),   // hasta yatagi (bed)
        new Rect(4f, 256f, 60f, 64f),     // yesil dolap (cabinet)
        new Rect(6f, 222f, 128f, 28f),    // uzun tezgah (bench)
        new Rect(4f, 84f, 52f, 60f),      // saksili bitki (potted plant)
        new Rect(86f, 174f, 64f, 42f),    // koyu masa (dark table)
        new Rect(60f, 96f, 36f, 42f),     // kirmizi cicek (red flowers)
    };

    public static void Decorate(Vector2 arenaMin, Vector2 arenaMax, int count)
    {
        Texture2D sheet = Resources.Load<Texture2D>(SheetResource);
        if (sheet == null)
        {
            return;
        }

        var sprites = new List<Sprite>(PropRects.Length);
        foreach (Rect rect in PropRects)
        {
            sprites.Add(Sprite.Create(sheet, rect, new Vector2(0.5f, 0.5f), 32f));
        }

        Transform parent = new GameObject("Props").transform;

        for (int i = 0; i < count; i++)
        {
            float x = Random.Range(arenaMin.x + 2f, arenaMax.x - 2f);
            float y = Random.Range(arenaMin.y + 2f, arenaMax.y - 2f);

            // Keep the middle clear so props don't land on the player's spawn.
            if (Mathf.Abs(x) < 3.5f && Mathf.Abs(y) < 3.5f)
            {
                continue;
            }

            GameObject prop = new GameObject("Prop");
            prop.transform.SetParent(parent, false);
            prop.transform.position = new Vector3(x, y, 1f);
            prop.layer = GameLayers.Prop;

            SpriteRenderer renderer = prop.AddComponent<SpriteRenderer>();
            renderer.sprite = sprites[Random.Range(0, sprites.Count)];
            renderer.sortingOrder = -5; // above the floor (-10), below characters (10+)

            // Solid: the player and enemies cannot walk through furniture.
            BoxCollider2D collider = prop.AddComponent<BoxCollider2D>();
            collider.size = renderer.sprite.bounds.size * 0.7f;
        }
    }
}
