using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Spikes : MonoBehaviour
{
    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other) { Hit(other); }
    void OnTriggerStay2D(Collider2D other) { Hit(other); }

    void Hit(Collider2D other)
    {
        var corpse = other.GetComponentInParent<PossessionObject>();
        if (corpse && !corpse.Impaled) corpse.Impale();
    }
}