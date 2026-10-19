using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class SlidingDoor : PoweredDevice
{
    public Vector2 openOffset = new Vector2(0f, 2f);
    public float speed = 4f;
    public bool carryRiders = true;

    [Header("Look")]
    public SpriteRenderer visual;
    public Sprite offSprite;
    public Sprite onSprite;

    Rigidbody2D rb;
    Collider2D col;
    Vector2 closedPos;
    bool wasOn;
    readonly HashSet<Rigidbody2D> carried = new HashSet<Rigidbody2D>();

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        closedPos = transform.position;
        wasOn = IsOn;
    }

    void Update()
    {
        if (IsOn != wasOn)
        {
            wasOn = IsOn;
            AudioController.PlayAt("door", transform.position);
        }

        if (!visual) return;
        Sprite s = IsOn ? onSprite : offSprite;
        if (s) visual.sprite = s;
    }

    void FixedUpdate()
    {
        Vector2 target = IsOn ? closedPos + openOffset : closedPos;
        Vector2 next = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
        float dx = next.x - rb.position.x;

        if (carryRiders && Mathf.Abs(dx) > 0.0001f)
            CarryRiders(dx);

        rb.MovePosition(next);
    }

    void CarryRiders(float dx)
    {
        carried.Clear();

        Bounds b = col.bounds;
        Vector2 center = new Vector2(b.center.x, b.max.y + 0.06f);
        Vector2 size = new Vector2(b.size.x * 0.95f, 0.12f);

        foreach (var h in Physics2D.OverlapBoxAll(center, size, 0f))
        {
            if (h.isTrigger) continue;

            var body = h.attachedRigidbody;
            if (!body || body == rb || body.bodyType != RigidbodyType2D.Dynamic) continue;

            if (carried.Add(body))
                body.position += new Vector2(dx, 0f);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!col) col = GetComponent<Collider2D>();
        if (!col) return;

        Vector3 start = Application.isPlaying ? (Vector3)closedPos : transform.position;
        Vector3 center = col.bounds.center - transform.position + start;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(center + (Vector3)openOffset, col.bounds.size);
        Gizmos.DrawLine(center, center + (Vector3)openOffset);
    }
}