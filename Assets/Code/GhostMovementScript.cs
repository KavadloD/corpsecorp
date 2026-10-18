using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class GhostMovementScript : MonoBehaviour
{
    public float speed = 5f;
    public float flyAcceleration = 30f;
    public float flyDrag = 12f;
    public float impaleEjectHeight = 1.5f;
    public float maxRange = 6f;

    Rigidbody2D rb;
    PossessionObject body;
    PossessionObject nearby;
    Vector2 movement;
    SpriteRenderer spriteRenderer;
    Animator animator;
    Collider2D ghostCol;
    Vector2 anchor;

    public Transform TetherTarget { get; private set; }
    public bool IsPossessing => body != null;
    public Vector2 Anchor => TetherTarget ? (Vector2)TetherTarget.position : anchor;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.simulated = true;
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        ghostCol = GetComponent<Collider2D>();
        ghostCol.isTrigger = true;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        anchor = transform.position;
    }

    void Update()
    {
        movement = new Vector2(
            (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0),
            (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0));

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (body) Leave();
            else if (nearby && !nearby.Impaled) Possess(nearby);
        }

        if (body && body.Impaled) Leave();

        if (!body && movement.x != 0)
            spriteRenderer.flipX = movement.x < 0;

        if (body)
        {
            if (movement.x != 0) body.Facing = (int)Mathf.Sign(movement.x);
            if (Input.GetKeyDown(KeyCode.W)) body.JumpPressed();
            body.JumpHeld = Input.GetKey(KeyCode.W);
            if (Input.GetKeyDown(KeyCode.Space)) body.DoAction();
            body.ActionHeld = Input.GetKey(KeyCode.Space);
        }
    }

    void FixedUpdate()
    {
        if (body)
        {
            if (!body.ControlLocked)
                body.Rb.linearVelocity = new Vector2(movement.x * body.CurrentSpeed, body.Rb.linearVelocity.y);
        }
        else
        {
            Vector2 target = movement.normalized * speed;
            float rate = movement.sqrMagnitude > 0 ? flyAcceleration : flyDrag;
            rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, target, rate * Time.fixedDeltaTime);

            Vector2 reach = rb.position + rb.linearVelocity * Time.fixedDeltaTime;
            Vector2 offset = reach - Anchor;
            if (offset.magnitude > maxRange)
            {
                Vector2 limitedPos = Anchor + offset.normalized * maxRange;
                rb.linearVelocity = Vector2.ClampMagnitude((limitedPos - rb.position) / Time.fixedDeltaTime, speed * 2f);
            }

            var limits = GhostBounds.Current;
            if (limits && limits.Contains(rb.position))
            {
                Vector2 next = rb.position + rb.linearVelocity * Time.fixedDeltaTime;
                Vector2 allowed = limits.Clamp(next, ghostCol.bounds.extents);
                rb.linearVelocity = Vector2.ClampMagnitude((allowed - rb.position) / Time.fixedDeltaTime, speed * 2f);
            }
        }
    }

    void LateUpdate()
    {
        float currentSpeed = rb.linearVelocity.magnitude;
        animator.SetFloat("Speed", currentSpeed);

        if (body) transform.position = body.transform.position;
    }

    void Possess(PossessionObject target)
    {
        body = target;
        body.SetPossessed(true);
        nearby = null;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        spriteRenderer.enabled = false;
    }

    void Leave()
    {
        Vector2 corpsePos = body.Rb.position;
        Vector2 pos = body.Impaled ? corpsePos + Vector2.up * impaleEjectHeight : corpsePos;

        var limits = GhostBounds.Current;
        if (limits) pos = limits.Clamp(pos, Vector2.one * 0.5f);

        transform.position = pos;
        anchor = corpsePos;
        TetherTarget = body.transform;

        nearby = body.Impaled ? null : body;
        body.SetPossessed(false);
        body = null;

        rb.simulated = true;
        rb.position = pos;
        rb.linearVelocity = Vector2.zero;
        spriteRenderer.enabled = true;

        // Debug.Log("Ghost left body at " + corpsePos + ", ghost placed at " + pos);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (body) return;
        var p = other.GetComponentInParent<PossessionObject>();
        if (p && !p.Impaled) nearby = p;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var p = other.GetComponentInParent<PossessionObject>();
        if (p && p == nearby) nearby = null;
    }

    public void ResetLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}