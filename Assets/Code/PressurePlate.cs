using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PressurePlate : MonoBehaviour
{
    public PoweredDevice[] devices;
    public bool pressedByBoxes = true;
    public float releaseDelay = 0.1f;

    public SpriteRenderer visual;
    public Sprite upSprite;
    public Sprite downSprite;
    public Color upColor = new Color(0.6f, 0.2f, 0.2f);
    public Color downColor = new Color(0.3f, 0.8f, 0.4f);

    Collider2D zone;
    bool pressed;
    float lastPressTime = -10f;

    public bool IsPressed => pressed;

    void Awake()
    {
        zone = GetComponent<Collider2D>();
        zone.isTrigger = true;
        Refresh();
    }

    void FixedUpdate()
    {
        if (Scan()) lastPressTime = Time.time;

        bool now = Time.time - lastPressTime <= releaseDelay;
        if (now == pressed) return;

        pressed = now;
        foreach (var d in devices)
            if (d) d.SetPowered(pressed);
        Refresh();
    }

    bool Scan()
    {
        Bounds b = zone.bounds;
        foreach (var h in Physics2D.OverlapBoxAll(b.center, b.size, 0f))
        {
            if (h == zone || h.isTrigger) continue;
            if (h.GetComponentInParent<PossessionObject>()) return true;
            if (pressedByBoxes && h.GetComponent<Pushable>()) return true;
        }
        return false;
    }

    void Refresh()
    {
        if (!visual) return;
        if (upSprite && downSprite) visual.sprite = pressed ? downSprite : upSprite;
        else visual.color = pressed ? downColor : upColor;
    }
}