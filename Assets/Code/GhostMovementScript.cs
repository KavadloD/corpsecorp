using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class GhostMovementScript : MonoBehaviour
{
    public float speed = 5f;

    Rigidbody2D rb;
    PossessionObject body;
    PossessionObject nearby;
    Vector2 movement;
    public float flyAcceleration = 30f;
    public float flyDrag = 12f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.simulated = true;
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        GetComponent<Collider2D>().isTrigger = true;
    }

    void Update()
    {
        movement = new Vector2(
            (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0),
            (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0));

        if (body)
        {
            if (Input.GetKeyDown(KeyCode.Space)) Leave();
        }
        else if (nearby && Input.GetKeyDown(KeyCode.Space))
        {
            Possess(nearby);
        }
    }

    void FixedUpdate()
    {
        if (body)
        {
            body.Rb.linearVelocity = new Vector2(movement.x * body.moveSpeed, body.Rb.linearVelocity.y);
        }
        else
        {
            Vector2 target = movement.normalized * speed;
            float rate = movement.sqrMagnitude > 0 ? flyAcceleration : flyDrag;
            rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, target, rate * Time.fixedDeltaTime);
        }
    }

    void LateUpdate()
    {
        if (body) transform.position = body.transform.position;
    }

    void Possess(PossessionObject target)
    {
        body = target;
        nearby = null;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
    }

    void Leave()
    {
        transform.position = body.transform.position;
        body.Rb.linearVelocity = new Vector2(0, body.Rb.linearVelocity.y);
        nearby = body;
        body = null;
        rb.simulated = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (body) return;
        var p = other.GetComponentInParent<PossessionObject>();
        if (p) nearby = p;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var p = other.GetComponentInParent<PossessionObject>();
        if (p && p == nearby) nearby = null;
    }
}