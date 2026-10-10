using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Pushable : MonoBehaviour
{
    const RigidbodyConstraints2D Locked = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
    const RigidbodyConstraints2D Free = RigidbodyConstraints2D.FreezeRotation;

    Rigidbody2D rb;
    Collider2D col;
    float unlockTimer;
    float carryTime = -1f;
    bool wasPushed;

    bool IsCarried => Time.fixedTime - carryTime <= 0.05f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    public void Push()
    {
        unlockTimer = 0.1f;
    }

    void FixedUpdate()
    {
        if (unlockTimer > 0)
        {
            unlockTimer -= Time.fixedDeltaTime;
            rb.constraints = Free;
            wasPushed = true;
            CarryRiders(rb.linearVelocity.x);
        }
        else if (!IsCarried)
        {
            rb.constraints = Locked;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            if (wasPushed)
            {
                wasPushed = false;
                ReleaseRiders();
            }
        }
    }

    void Carry(float vx)
    {
        carryTime = Time.fixedTime;
        rb.constraints = Free;
        rb.linearVelocity = new Vector2(vx, rb.linearVelocity.y);
        CarryRiders(vx);
    }

    void Release()
    {
        carryTime = -1f;
        rb.constraints = Locked;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        ReleaseRiders();
    }

    void CarryRiders(float vx)
    {
        foreach (var h in FindRiders())
        {
            var box = h.GetComponent<Pushable>();
            if (box) { box.Carry(vx); continue; }
            var corpse = h.GetComponentInParent<PossessionObject>();
            if (corpse) corpse.Carry(vx);
        }
    }

    void ReleaseRiders()
    {
        foreach (var h in FindRiders())
        {
            var box = h.GetComponent<Pushable>();
            if (box) { box.Release(); continue; }
            var corpse = h.GetComponentInParent<PossessionObject>();
            if (corpse) corpse.StopCarry();
        }
    }

    Collider2D[] FindRiders()
    {
        Bounds b = col.bounds;
        Vector2 center = new Vector2(b.center.x, b.max.y + 0.05f);
        Vector2 size = new Vector2(b.size.x * 0.9f, 0.1f);
        var all = Physics2D.OverlapBoxAll(center, size, 0f);
        var result = new System.Collections.Generic.List<Collider2D>();
        foreach (var h in all)
            if (h != col) result.Add(h);
        return result.ToArray();
    }
}