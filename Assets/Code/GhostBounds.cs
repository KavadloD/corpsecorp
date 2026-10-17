using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class GhostBounds : MonoBehaviour
{
    public static GhostBounds Current { get; private set; }

    BoxCollider2D box;

    void Awake()
    {
        Current = this;
        box = GetComponent<BoxCollider2D>();
        box.isTrigger = true;
    }

    void OnDestroy()
    {
        if (Current == this) Current = null;
    }

    public Vector2 Clamp(Vector2 p, Vector2 halfSize)
    {
        Bounds b = box.bounds;
        float x = Mathf.Clamp(p.x, b.min.x + halfSize.x, b.max.x - halfSize.x);
        float y = Mathf.Clamp(p.y, b.min.y + halfSize.y, b.max.y - halfSize.y);
        return new Vector2(x, y);
    }

    void OnDrawGizmos()
    {
        var b = GetComponent<BoxCollider2D>();
        if (!b) return;
        Vector3 center = transform.TransformPoint(b.offset);
        Vector3 size = Vector3.Scale(b.size, transform.lossyScale);
        Gizmos.color = new Color(1f, 0.4f, 0.8f, 0.8f);
        Gizmos.DrawWireCube(center, size);
    }
}