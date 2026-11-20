using UnityEngine;

namespace GGFramework.GGUI.UGUI
{
    public static class ViewportGeometry
    {
        public static float Fit(Vector2 content, Vector2 viewport) =>
            Mathf.Min(viewport.x / content.x, viewport.y / content.y);

        public static Vector2 Clamp(Vector2 position, Vector2 content, Vector2 viewport)
        {
            var excess = Vector2.Max(Vector2.zero, (content - viewport) * .5f);
            return new Vector2(Mathf.Clamp(position.x, -excess.x, excess.x),
                               Mathf.Clamp(position.y, -excess.y, excess.y));
        }
    }
}
