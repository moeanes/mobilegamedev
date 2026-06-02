using UnityEngine;

// Plain data describing one level. LevelDatabase holds the five of these.
[System.Serializable]
public class LevelData
{
    public int levelIndex;
    public string displayName;
    public int enemiesToKill;
    public float spawnInterval;
    public int maxConcurrentAlive;

    // 0..1 chance to spawn a ranged enemy instead of a melee one.
    public float rangedWeight;
    public int rangedFromLevel;

    public string floorTileResource; // Resources path of the tiled floor background
    public Color floorTint;
}
