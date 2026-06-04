using UnityEngine;

// The five levels, hardcoded. PendingLevel is the one the next loaded Game scene builds.
public static class LevelDatabase
{
    public const int FinalLevel = 5;

    public static int PendingLevel { get; private set; } = 1;

    public static void SetPendingLevel(int level)
    {
        PendingLevel = Mathf.Clamp(level, 1, FinalLevel);
    }

    public static void ResetToFirstLevel()
    {
        PendingLevel = 1;
    }

    public static LevelData Get(int level)
    {
        int clamped = Mathf.Clamp(level, 1, FinalLevel);
        switch (clamped)
        {
            case 1:
                return new LevelData
                {
                    levelIndex = 1, displayName = "Giris Kati", enemiesToKill = 6,
                    spawnInterval = 1.15f, maxConcurrentAlive = 3,
                    rangedWeight = 0f, rangedFromLevel = 2,
                    floorTileResource = "Tiles/lab_floor", floorTint = new Color(0.80f, 0.86f, 0.92f, 1f),
                };
            case 2:
                return new LevelData
                {
                    levelIndex = 2, displayName = "Laboratuvar", enemiesToKill = 8,
                    spawnInterval = 1.05f, maxConcurrentAlive = 4,
                    rangedWeight = 0.15f, rangedFromLevel = 2,
                    floorTileResource = "Tiles/lab_floor", floorTint = new Color(0.74f, 0.86f, 0.80f, 1f),
                };
            case 3:
                return new LevelData
                {
                    levelIndex = 3, displayName = "Servis", enemiesToKill = 11,
                    spawnInterval = 1.0f, maxConcurrentAlive = 4,
                    rangedWeight = 0.25f, rangedFromLevel = 2,
                    floorTileResource = "Tiles/lab_floor", floorTint = new Color(0.86f, 0.82f, 0.90f, 1f),
                };
            case 4:
                return new LevelData
                {
                    levelIndex = 4, displayName = "Yogun Bakim", enemiesToKill = 14,
                    spawnInterval = 0.95f, maxConcurrentAlive = 5,
                    rangedWeight = 0.30f, rangedFromLevel = 2,
                    floorTileResource = "Tiles/lab_floor", floorTint = new Color(0.92f, 0.80f, 0.80f, 1f),
                };
            default:
                return new LevelData
                {
                    levelIndex = 5, displayName = "Son Kat", enemiesToKill = 18,
                    spawnInterval = 0.85f, maxConcurrentAlive = 5,
                    rangedWeight = 0.35f, rangedFromLevel = 2,
                    floorTileResource = "Tiles/lab_floor", floorTint = new Color(0.94f, 0.74f, 0.74f, 1f),
                    hasBoss = true,
                };
        }
    }
}
