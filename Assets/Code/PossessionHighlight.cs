using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PossessionObject))]
public class PossessionHighlight : MonoBehaviour
{
    public Material solidMaterial;
    public Color darkTint = new Color(0.55f, 0.55f, 0.65f);
    public Color possessedTint = Color.white;
    public Color glowColor = new Color(0.6f, 0.85f, 1f);
    public float outerAlpha = 0.25f;
    public float pulse = 0.15f;
    public float fadeTime = 0.12f;
    public float innerAlpha = 0.6f;

    static readonly Vector2[] Dirs =
    {
        new Vector2(1, 0), new Vector2(-1, 0), new Vector2(0, 1), new Vector2(0, -1),
        new Vector2(1, 1), new Vector2(1, -1), new Vector2(-1, 1), new Vector2(-1, -1)
    };

    class Copy
    {
        public SpriteRenderer sr;
        public Vector2 dir;
        public int ring;
    }

    PossessionObject po;
    SpriteRenderer[] tinted;
    SpriteRenderer source;
    readonly List<Copy> inner = new List<Copy>();
    readonly List<Copy> outer = new List<Copy>();
    float amount;
    float wobble = 1f;

    void Awake()
    {
        po = GetComponent<PossessionObject>();

        var list = new List<SpriteRenderer>();
        if (po.normalVisual) list.AddRange(po.normalVisual.GetComponentsInChildren<SpriteRenderer>(true));
        if (po.impaledVisual) list.AddRange(po.impaledVisual.GetComponentsInChildren<SpriteRenderer>(true));
        if (list.Count == 0) list.AddRange(GetComponentsInChildren<SpriteRenderer>(true));
        tinted = list.ToArray();

        if (tinted.Length == 0)
        {
            enabled = false;
            return;
        }

        if (!solidMaterial)
            Debug.LogWarning("PossessionHighlight: assign M_OutlineSolid to Solid Material on " + name);

        source = tinted[0];
        BuildRing(2, outer, source.sortingOrder - 2);
        BuildRing(1, inner, source.sortingOrder - 1);
    }

    void BuildRing(int ring, List<Copy> into, int order)
    {
        foreach (var d in Dirs)
        {
            var go = new GameObject("Outline");
            go.transform.SetParent(source.transform, false);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sharedMaterial = solidMaterial ? solidMaterial : source.sharedMaterial;
            sr.sortingLayerID = source.sortingLayerID;
            sr.sortingOrder = order;
            sr.enabled = false;

            into.Add(new Copy { sr = sr, dir = d, ring = ring });
        }
    }

    void Update()
    {
        float target = (po.IsPossessed && !po.Impaled) ? 1f : 0f;
        amount = Mathf.MoveTowards(amount, target, Time.deltaTime / Mathf.Max(0.01f, fadeTime));
        wobble = 1f + pulse * Mathf.Sin(Time.time * 4f);

        Color tint = Color.Lerp(darkTint, possessedTint, amount);
        foreach (var sr in tinted)
            if (sr) sr.color = tint;
    }

    void LateUpdate()
    {
        bool show = amount > 0.01f && source.sprite != null;

        Color innerColor = glowColor;
        innerColor.a = innerAlpha * amount;
        Color outerColor = glowColor;
        outerColor.a = Mathf.Clamp01(outerAlpha * amount * wobble);

        Sync(inner, show, innerColor);
        Sync(outer, show, outerColor);
    }

    void Sync(List<Copy> list, bool show, Color color)
    {
        float ppu = source.sprite ? source.sprite.pixelsPerUnit : 16f;
        Vector3 s = source.transform.lossyScale;

        foreach (var c in list)
        {
            c.sr.enabled = show;
            if (!show) continue;

            c.sr.sprite = source.sprite;
            c.sr.flipX = source.flipX;
            c.sr.flipY = source.flipY;
            c.sr.color = color;
            c.sr.transform.localPosition = new Vector3(
                c.dir.x * c.ring / ppu / s.x,
                c.dir.y * c.ring / ppu / s.y,
                0f);
        }
    }
}