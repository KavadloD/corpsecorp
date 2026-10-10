using UnityEngine;

public class JumperCorpse : PossessionObject
{
    public float bigJumpHeight = 10.5f;
    public float releaseCut = 0.4f;

    bool jumping;

    public override void DoAction()
    {
        if (!IsGrounded) return;
        JumpWithHeight(bigJumpHeight);
        jumping = true;
    }

    void FixedUpdate()
    {
        if (!jumping) return;

        if (Rb.linearVelocity.y <= 0f)
        {
            jumping = false;
        }
        else if (!ActionHeld)
        {
            Rb.linearVelocity = new Vector2(Rb.linearVelocity.x, Rb.linearVelocity.y * releaseCut);
            jumping = false;
        }
    }
}