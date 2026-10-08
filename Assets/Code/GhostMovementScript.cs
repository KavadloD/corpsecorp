using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class GhostMovementScript : MonoBehaviour
{
    public float speed = 5f;
    Rigidbody2D rb;
    PossessionObject body;
    Vector2 movement;
    bool leaving;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        GetComponent<Collider2D>().isTrigger = true;
    }

    void Update()
    {
        movement = new Vector2((Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0),
            (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0));
        leaving = Input.GetKey(KeyCode.Q);
        if (body && leaving)
        {
            transform.position = body.transform.position;
            body.Rb.linearVelocity = new Vector2(0, body.Rb.linearVelocity.y);
            body = null;
            rb.simulated = true;
        }
    }

    void FixedUpdate()
    {
        if (body) body.Rb.linearVelocity = new Vector2(movement.x * speed, body.Rb.linearVelocity.y);
        else rb.linearVelocity = movement.normalized * speed;
    }

    void LateUpdate()
    {
        if (body) transform.position = body.transform.position;
        else if (!rb.simulated) rb.simulated = true;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (body || leaving) return;
        body = other.GetComponentInParent<PossessionObject>();
        if (body) rb.simulated = false;
    }
}
