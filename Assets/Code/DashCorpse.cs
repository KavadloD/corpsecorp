using System.Collections.Generic;
using UnityEngine;

public class DashCorpse : PossessionObject
{
    public float dashSpeed = 14f;
    public float dashDuration = 0.18f;
    public float dashCooldown = 0.6f;

    float dashTimer;
    float cooldownTimer;
    int dashDir = 1;
    readonly HashSet<Breakable> targets = new HashSet<Breakable>();

    public override void DoAction()
    {
        if (dashTimer > 0 || cooldownTimer > 0) return;
        dashDir = Facing;
        dashTimer = dashDuration;
        cooldownTimer = dashCooldown;
        ControlLocked = true;
        CaptureTargets();
    }

    void CaptureTargets()
    {
        targets.Clear();
        Bounds b = col.bounds;
        float reach = dashSpeed * dashDuration;
        Vector2 center = new Vector2(b.center.x + dashDir * (b.extents.x + reach / 2f), b.center.y);
        Vector2 size = new Vector2(reach, b.size.y * 0.9f);

        foreach (var hit in Physics2D.OverlapBoxAll(center, size, 0f))
        {
            var br = hit.GetComponent<Breakable>();
            if (br) targets.Add(br);
        }
    }

    void FixedUpdate()
    {
        if (cooldownTimer > 0) cooldownTimer -= Time.fixedDeltaTime;
        if (dashTimer <= 0) return;

        dashTimer -= Time.fixedDeltaTime;
        Rb.linearVelocity = new Vector2(dashDir * dashSpeed, 0f);

        if (dashTimer <= 0)
        {
            ControlLocked = false;
            targets.Clear();
            Rb.linearVelocity = new Vector2(0f, Rb.linearVelocity.y);
        }
    }

    void OnCollisionEnter2D(Collision2D c) { TryBreak(c); }
    void OnCollisionStay2D(Collision2D c) { TryBreak(c); }

    void TryBreak(Collision2D c)
    {
        if (dashTimer <= 0) return;
        var b = c.collider.GetComponent<Breakable>();
        if (b && targets.Remove(b)) b.Break();
    }
}