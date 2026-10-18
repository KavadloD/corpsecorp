using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GhostTether : MonoBehaviour
{
    public GhostMovementScript ghost;
    public Color relaxedColor = new Color(0.6f, 0.85f, 1f, 0.35f);
    public Color strainedColor = new Color(1f, 0.45f, 0.45f, 0.95f);
    public float width = 0.08f;
    public int points = 16;
    public float sway = 0.25f;
    public float showFrom = 0.3f;

    LineRenderer line;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        if (!ghost) ghost = GetComponent<GhostMovementScript>();

        line.useWorldSpace = true;
        line.positionCount = points;
        line.startWidth = width;
        line.endWidth = width;
        line.enabled = false;
    }

    void LateUpdate()
    {
        if (!ghost || ghost.IsPossessing || ghost.TetherTarget == null)
        {
            line.enabled = false;
            return;
        }

        Vector2 a = ghost.transform.position;
        Vector2 b = ghost.Anchor;
        float strain = Mathf.Clamp01(Vector2.Distance(a, b) / Mathf.Max(0.1f, ghost.maxRange));

        float visible = Mathf.InverseLerp(showFrom, 1f, strain);
        if (visible <= 0f)
        {
            line.enabled = false;
            return;
        }
        line.enabled = true;

        Color c = Color.Lerp(relaxedColor, strainedColor, strain * strain);
        c.a *= visible;
        if (strain > 0.9f) c.a *= 0.8f + 0.2f * Mathf.Sin(Time.time * 25f);
        line.startColor = c;
        line.endColor = c;

        Vector2 dir = b - a;
        Vector2 perp = dir.sqrMagnitude > 0.0001f ? new Vector2(-dir.y, dir.x).normalized : Vector2.up;
        float amp = sway * (1f - strain);

        for (int i = 0; i < points; i++)
        {
            float t = i / (float)(points - 1);
            Vector2 p = Vector2.Lerp(a, b, t);
            float bend = Mathf.Sin(t * Mathf.PI) * amp * Mathf.Sin(Time.time * 3f + t * 6f);
            p += perp * bend;
            line.SetPosition(i, new Vector3(p.x, p.y, 0f));
        }
    }
}