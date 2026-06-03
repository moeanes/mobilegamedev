using UnityEngine;

// Lightweight chase steering for enemies that have no real pathfinding. They head
// straight at the target, but when a wall is directly ahead they pick the next-best
// direction (turning a little, then more) so they slide along the wall and round
// corners into doorways instead of pressing uselessly into it.
public static class EnemyNavigation
{
    private static readonly int WallMask = 1 << GameLayers.Wall;
    private const float LookAhead = 0.7f;

    // Directions to try, in order of preference: straight on, then gentle turns, then
    // sharp turns, then sideways. The first one clear of walls wins.
    private static readonly float[] TurnAngles = { 0f, 35f, -35f, 70f, -70f, 110f, -110f };

    public static Vector2 Steer(Vector2 position, Vector2 desired, float radius)
    {
        if (desired.sqrMagnitude < 0.0001f)
        {
            return Vector2.zero;
        }

        desired.Normalize();

        foreach (float angle in TurnAngles)
        {
            Vector2 candidate = Rotate(desired, angle);
            if (!Blocked(position, candidate, radius))
            {
                return candidate;
            }
        }

        return Vector2.zero;
    }

    private static bool Blocked(Vector2 from, Vector2 direction, float radius)
        => Physics2D.Raycast(from, direction, radius + LookAhead, WallMask).collider != null;

    private static Vector2 Rotate(Vector2 v, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }
}
