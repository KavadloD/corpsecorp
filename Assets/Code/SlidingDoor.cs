using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class SlidingDoor : PoweredDevice
{
    public Vector2 openOffset = new Vector2(0f, 2f);
    public float speed = 4f;

    [Header("Look")]
    public SpriteRenderer visual;
    public Sprite offSprite;
    public Sprite onSprite;

    Rigidbody2D rb;
    Vector2 closedPos;
    bool wasOn;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
        rb.MovePosition(Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime));
    }

    void OnDrawGizmosSelected()
    {
        var col = GetComponent<Collider2D>();
        if (!col) return;

        Vector3 start = Application.isPlaying ? (Vector3)closedPos : transform.position;
        Vector3 center = col.bounds.center - transform.position + start;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(center + (Vector3)openOffset, col.bounds.size);
        Gizmos.DrawLine(center, center + (Vector3)openOffset);
    }
}