using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PossessionObject : MonoBehaviour
{
    public float gravity = 3f;
    public float moveSpeed = 3f;
    public float jumpHeight = 1.25f;
    public LayerMask groundMask;
    public Rigidbody2D Rb { get; private set; }
    protected Collider2D col;
    public int Facing { get; set; } = 1;
    public bool ControlLocked { get; protected set; }
    public virtual void DoAction() { }
    public virtual bool IsActing => false;
    public bool ActionHeld { get; set; }
    public virtual float CurrentSpeed => moveSpeed;
    public bool IsPossessed { get; private set; }
    float carryTime = -1f;
    public bool Impaled { get; private set; }

    public GameObject normalVisual;
    public GameObject impaledVisual;
    public float impaledHeight = 1f;
    public float impaledWidth = 1f;
    public Vector2 impaledColliderSize = new Vector2(1f, 0.45f);
    public Vector2 impaledColliderOffset = new Vector2(0f, -0.7f);
    public bool JumpHeld { get; set; }
    public virtual void JumpPressed() { Jump(); }

    void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
        Rb.bodyType = RigidbodyType2D.Dynamic;
        Rb.gravityScale = gravity;
        SetPossessed(false);
        GetComponent<Collider2D>().isTrigger = false;
        col = GetComponent<Collider2D>();
    }

    public bool IsGrounded
    {
        get
        {
            Bounds b = col.bounds;
            Vector2 origin = new Vector2(b.center.x, b.min.y - 0.05f);
            Vector2 size = new Vector2(b.size.x * 0.9f, 0.1f);
            return Physics2D.OverlapBox(origin, size, 0f, groundMask);
        }
    }

    public void Jump()
    {
        JumpWithHeight(jumpHeight);
    }

    public void JumpWithHeight(float height)
    {
        if (!IsGrounded) return;
        float v = Mathf.Sqrt(2f * Physics2D.gravity.magnitude * Rb.gravityScale * height);
        Rb.linearVelocity = new Vector2(Rb.linearVelocity.x, v);
    }

    public void SetPossessed(bool possessed)
    {
        if (Impaled) return;
        IsPossessed = possessed;
        Rb.constraints = possessed
            ? RigidbodyConstraints2D.FreezeRotation
            : RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

        if (!possessed)
        {
            ActionHeld = false;
            JumpHeld = false;
            Rb.linearVelocity = new Vector2(0f, Rb.linearVelocity.y);
        }
    }

    public void Carry(float vx)
    {
        if (IsPossessed || Impaled) return;
        carryTime = Time.fixedTime;
        Rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        Rb.linearVelocity = new Vector2(vx, Rb.linearVelocity.y);
    }

    public void StopCarry()
    {
        carryTime = -1f;
        if (!IsPossessed) SetPossessed(false);
    }

    void LateUpdate()
    {
        if (carryTime >= 0f && Time.fixedTime - carryTime > 0.1f) StopCarry();
    }

    public void Impale(float restY = float.NaN)
    {
        if (Impaled) return;
        Impaled = true;
        IsPossessed = false;
        ActionHeld = false;
        ControlLocked = true;

        // Measure and snap to the floor before changing layers or colliders
        Bounds b = col.bounds;
        Vector3 s = transform.lossyScale;
        float localBottom = (b.min.y - transform.position.y) / s.y;
        float localCenterX = (b.center.x - transform.position.x) / s.x;

        float dy = 0f;
        if (!float.IsNaN(restY))
        {
            dy = restY - b.min.y;
        }
        else
        {
            RaycastHit2D hit = Physics2D.Raycast(
                new Vector2(b.center.x, b.min.y + 0.1f), Vector2.down, 1.5f, groundMask);
            if (hit) dy = hit.point.y - b.min.y;
        }

        if (Mathf.Abs(dy) > 1.5f)
        {
            // Debug.LogWarning("Impale snap of " + dy + " ignored on " + name + " (spike top " + restY + ", body bottom " + b.min.y + ")");
            dy = 0f;
        }
        Rb.position += Vector2.up * dy;

        Rb.linearVelocity = Vector2.zero;
        Rb.constraints = RigidbodyConstraints2D.FreezeAll;
        gameObject.layer = LayerMask.NameToLayer("Ground");

        // 1 block tall solid box, bottom edge where the old collider's bottom was
        var box = gameObject.AddComponent<BoxCollider2D>();
        box.sharedMaterial = col.sharedMaterial;
        box.size = impaledColliderSize;
        box.offset = impaledColliderOffset;
        col.enabled = false;

        if (normalVisual) normalVisual.SetActive(false);
        if (impaledVisual) impaledVisual.SetActive(true);
    }
}
