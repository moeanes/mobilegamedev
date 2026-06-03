using UnityEngine;

// Draws a texture as a clickable IMGUI button, centred horizontally, keeping the image's
// aspect ratio and growing slightly on hover. Shared by the menu and the end screens.
public static class UiButton
{
    public static float Height(Texture2D texture, float width)
        => texture != null ? width * texture.height / texture.width : width * 0.3f;

    public static bool Draw(Texture2D texture, float centerX, float y, float width)
    {
        float height = Height(texture, width);
        Rect rect = new Rect(centerX - width * 0.5f, y, width, height);

        bool hover = rect.Contains(Event.current.mousePosition);
        Rect drawRect = hover ? Grow(rect, 1.06f) : rect;

        if (texture != null)
        {
            return GUI.Button(drawRect, texture, GUIStyle.none);
        }

        return GUI.Button(drawRect, string.Empty);
    }

    private static Rect Grow(Rect rect, float scale)
    {
        float deltaWidth = rect.width * (scale - 1f);
        float deltaHeight = rect.height * (scale - 1f);
        return new Rect(rect.x - deltaWidth * 0.5f, rect.y - deltaHeight * 0.5f, rect.width + deltaWidth, rect.height + deltaHeight);
    }
}
