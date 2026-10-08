using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PossessionObject : MonoBehaviour
{
    public float gravity = 3f;
    public Rigidbody2D Rb { get; private set; }

    void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
        Rb.bodyType = RigidbodyType2D.Dynamic;
        Rb.gravityScale = gravity;
        Rb.freezeRotation = true;
        GetComponent<Collider2D>().isTrigger = false;
    }
}
