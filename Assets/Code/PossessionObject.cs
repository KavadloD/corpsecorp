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
    public bool ActionHeld { get; set; }
    public virtual float CurrentSpeed => moveSpeed;
    public bool IsPossessed { get; private set; }
    float carryTime = -1f;

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
        IsPossessed = possessed;
        Rb.constraints = possessed
            ? RigidbodyConstraints2D.FreezeRotation
            : RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

        if (!possessed)
        {
            ActionHeld = false;
            Rb.linearVelocity = new Vector2(0f, Rb.linearVelocity.y);
        }
    }

    public void Carry(float vx)
    {
        if (IsPossessed) return;
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
}
