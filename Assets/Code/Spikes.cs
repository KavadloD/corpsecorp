using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Spikes : MonoBehaviour
{
    Collider2D col;
    public float restOffset = -0.0625f;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other) { Hit(other); }
    void OnTriggerStay2D(Collider2D other) { Hit(other); }

    void Hit(Collider2D other)
    {
        var corpse = other.GetComponentInParent<PossessionObject>();
        if (corpse && !corpse.Impaled) corpse.Impale(col.bounds.max.y + restOffset);
    }
}