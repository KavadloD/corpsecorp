using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Generator : MonoBehaviour
{
    public PoweredDevice[] devices;
    public SpriteRenderer visual;
    public Color offColor = new Color(0.5f, 0.5f, 0.5f);
    public Color onColor = Color.white;
    public Sprite[] onFrames;
    public float frameRate = 6f;

    Sprite offSprite;
    float animTimer;

    public bool IsOn { get; private set; }

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        if (visual) offSprite = visual.sprite;
        Refresh();
    }

    void Update()
    {
        if (!IsOn || !visual || onFrames == null || onFrames.Length == 0) return;

        animTimer += Time.deltaTime * frameRate;
        visual.sprite = onFrames[(int)animTimer % onFrames.Length];
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (IsOn) return;

        var corpse = other.GetComponentInParent<PossessionObject>();
        if (!corpse || !corpse.IsPossessed) return;

        IsOn = true;
        foreach (var d in devices)
            if (d) d.SetPowered(true);
        Refresh();
    }

    void Refresh()
    {
        if (!visual) return;
        visual.color = IsOn ? onColor : offColor;
        if (!IsOn && offSprite) visual.sprite = offSprite;
    }
}