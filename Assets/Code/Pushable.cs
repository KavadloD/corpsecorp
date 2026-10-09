using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Pushable : MonoBehaviour
{
    Rigidbody2D rb;
    float unlockTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        else
        {
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }
}