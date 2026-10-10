using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Generator : MonoBehaviour
{
    public PoweredDevice[] devices;
    public SpriteRenderer visual;
    public Color offColor = new Color(0.5f, 0.5f, 0.5f);
    public Color onColor = new Color(1f, 0.9f, 0.3f);

    public bool IsOn { get; private set; }

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        Refresh();
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
        if (visual) visual.color = IsOn ? onColor : offColor;
    }
}