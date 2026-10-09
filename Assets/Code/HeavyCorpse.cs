using UnityEngine;

public class HeavyCorpse : PossessionObject
{
    public float pushSpeed = 1.5f;

    public override float CurrentSpeed => ActionHeld ? pushSpeed : moveSpeed;

    void OnCollisionEnter2D(Collision2D c) { TryPush(c); }
    void OnCollisionStay2D(Collision2D c) { TryPush(c); }

    void TryPush(Collision2D c)
    {
        if (!ActionHeld) return;
        var p = c.collider.GetComponent<Pushable>();
        if (!p) return;
        if (Mathf.Abs(c.GetContact(0).normal.x) < 0.5f) return;
        p.Push();
    }
}