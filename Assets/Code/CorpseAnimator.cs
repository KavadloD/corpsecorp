using UnityEngine;

[RequireComponent(typeof(PossessionObject))]
public class CorpseAnimator : MonoBehaviour
{
    public SpriteRenderer visual;
    public Sprite[] idleFrames;
    public Sprite[] walkFrames;
    public Sprite jumpSprite;
    public Sprite actionSprite;
    public Sprite[] actionFrames;
    public float idleFps = 2.5f;
    public float walkFps = 8f;
    public float actionFps = 5f;
    public bool artFacesRight = true;

    PossessionObject po;
    int lastState = -1;
    float timer;
    float airTime;

    void Awake()
    {
        po = GetComponent<PossessionObject>();
        if (!visual && po.normalVisual)
            visual = po.normalVisual.GetComponentInChildren<SpriteRenderer>(true);
        if (!visual)
            visual = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        if (!visual || po.Impaled) return;

        airTime = po.IsGrounded ? 0f : airTime + Time.deltaTime;
        bool moving = Mathf.Abs(po.Rb.linearVelocity.x) > 0.1f;
        bool hasAction = actionSprite || (actionFrames != null && actionFrames.Length > 0);

        // 0 idle, 1 walk, 2 air, 3 action
        int state;
        if (po.IsActing && hasAction) state = 3;
        else if (airTime > 0.06f && jumpSprite) state = 2;
        else if (po.IsPossessed && moving && walkFrames != null && walkFrames.Length > 0) state = 1;
        else state = 0;

        if (state != lastState)
        {
            lastState = state;
            timer = 0f;
        }
        timer += Time.deltaTime;

        switch (state)
        {
            case 3:
                if (actionFrames != null && actionFrames.Length > 0)
                    visual.sprite = (po.IsPossessed && moving) ? Pick(actionFrames, actionFps) : actionFrames[0];
                else
                    visual.sprite = actionSprite;
                break;
            case 2:
                visual.sprite = jumpSprite;
                break;
            case 1:
                visual.sprite = Pick(walkFrames, walkFps);
                break;
            default:
                if (idleFrames != null && idleFrames.Length > 0)
                    visual.sprite = po.IsPossessed ? Pick(idleFrames, idleFps) : idleFrames[0];
                break;
        }

        visual.flipX = artFacesRight ? po.Facing < 0 : po.Facing > 0;
    }

    Sprite Pick(Sprite[] frames, float fps)
    {
        return frames[(int)(timer * fps) % frames.Length];
    }
}