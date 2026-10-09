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

    void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
        Rb.bodyType = RigidbodyType2D.Dynamic;
        Rb.gravityScale = gravity;
        Rb.freezeRotation = true;
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
        if (!IsGrounded) return;
        float v = Mathf.Sqrt(2f * Physics2D.gravity.magnitude * Rb.gravityScale * jumpHeight);
        Rb.linearVelocity = new Vector2(Rb.linearVelocity.x, v);
    }
}
