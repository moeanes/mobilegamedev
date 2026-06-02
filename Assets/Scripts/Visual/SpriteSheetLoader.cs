using System.Collections.Generic;
using UnityEngine;

// Loads a pixel-art sheet from Resources and cuts it into equal frames at runtime.
// No editor slicing needed: each frame is a Sprite over one sub-rect of the texture.
public static class SpriteSheetLoader
{
    private const int PixelsPerUnit = 32;
    private static readonly Dictionary<string, Sprite[]> Cache = new Dictionary<string, Sprite[]>();

    // maxFrames > 0 keeps only the first N frames (some sheets have blank trailing
    // cells — e.g. enemy1 is a 3x3 grid but only 7 cells are drawn).
    public static Sprite[] Load(string resourcePath, int frameWidth, int frameHeight, int maxFrames = 0)
    {
        string key = resourcePath + ":" + frameWidth + "x" + frameHeight + ":" + maxFrames;
        if (Cache.TryGetValue(key, out Sprite[] cached))
        {
            return cached;
        }

        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null)
        {
            Debug.LogWarning("[SpriteSheetLoader] Texture bulunamadi: " + resourcePath);
            Cache[key] = new Sprite[0];
            return Cache[key];
        }

        int columns = Mathf.Max(1, texture.width / frameWidth);
        int rows = Mathf.Max(1, texture.height / frameHeight);

        int total = columns * rows;
        if (maxFrames > 0)
        {
            total = Mathf.Min(total, maxFrames);
        }

        var frames = new List<Sprite>(total);

        // Top-to-bottom, left-to-right so frames read in the same order as the sheet.
        for (int index = 0; index < total; index++)
        {
            int row = index / columns;
            int column = index % columns;
            var rect = new Rect(
                column * frameWidth,
                texture.height - (row + 1) * frameHeight,
                frameWidth,
                frameHeight);
            frames.Add(Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), PixelsPerUnit));
        }

        Cache[key] = frames.ToArray();
        return Cache[key];
    }

    public static Sprite LoadSingle(string resourcePath, int frameWidth, int frameHeight, int index)
    {
        Sprite[] frames = Load(resourcePath, frameWidth, frameHeight);
        if (frames.Length == 0)
        {
            return null;
        }

        return frames[Mathf.Clamp(index, 0, frames.Length - 1)];
    }
}
