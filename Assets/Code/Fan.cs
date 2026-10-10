using UnityEngine;

public class Fan : PoweredDevice
{
    public Collider2D liftZone;
    public float lift = 45f;
    public float maxRiseSpeed = 6f;
    public SpriteRenderer visual;
    public Color offColor = new Color(0.5f, 0.5f, 0.5f);
    public Color onColor = new Color(0.4f, 0.9f, 1f);

    Rigidbody2D selfRb;

    void Awake()
    {
        selfRb = GetComponent<Rigidbody2D>();
        if (liftZone) liftZone.isTrigger = true;
    }

    void Update()
    {
        if (visual) visual.color = IsOn ? onColor : offColor;
    }

    void FixedUpdate()
    {
        if (!IsOn || !liftZone) return;

        Bounds b = liftZone.bounds;
        foreach (var h in Physics2D.OverlapBoxAll(b.center, b.size, 0f))
        {
            var rb = h.attachedRigidbody;
            if (!rb || rb == selfRb || rb.bodyType != RigidbodyType2D.Dynamic) continue;
            if (!h.GetComponentInParent<PossessionObject>() && !h.GetComponent<Pushable>()) continue;
            if (rb.linearVelocity.y < maxRiseSpeed) rb.AddForce(Vector2.up * lift);
        }
    }
}