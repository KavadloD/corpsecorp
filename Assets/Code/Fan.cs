using UnityEngine;

public class Fan : PoweredDevice
{
    public Collider2D liftZone;
    public float lift = 45f;
    public float maxRiseSpeed = 6f;

    [Header("Look")]
    public SpriteRenderer visual;
    public Sprite offSprite;
    public Sprite[] onFrames;
    public float frameRate = 6f;

    [Header("Wind particles")]
    public ParticleSystem wind;
    public float particleSpeed = 3f;
    public float particlesPerSecond = 14f;
    public Color windColor = new Color(0.8f, 0.95f, 1f, 0.7f);

    Rigidbody2D selfRb;
    float animTimer;
    LoopingSound loop;

    void Awake()
    {
        selfRb = GetComponent<Rigidbody2D>();
        if (liftZone) liftZone.isTrigger = true;
        loop = GetComponent<LoopingSound>();
    }

    void Start()
    {
        if (!wind || !liftZone) return;

        Bounds b = liftZone.bounds;
        wind.transform.position = new Vector3(b.center.x, b.min.y, wind.transform.position.z);

        var main = wind.main;
        main.loop = true;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.startSpeed = 0f;
        main.startLifetime = b.size.y / particleSpeed;
        main.startSize = new ParticleSystem.MinMaxCurve(0.0625f, 0.125f);
        main.startColor = windColor;
        main.gravityModifier = 0f;
        main.maxParticles = 100;

        var shape = wind.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.rotation = Vector3.zero;
        shape.scale = new Vector3(b.size.x * 0.9f, 0.05f, 0.01f);

        var vel = wind.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.Local;
        vel.x = new ParticleSystem.MinMaxCurve(0f);
        vel.y = new ParticleSystem.MinMaxCurve(particleSpeed);
        vel.z = new ParticleSystem.MinMaxCurve(0f);

        var fade = wind.colorOverLifetime;
        fade.enabled = true;
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(0f, 1f) });
        fade.color = g;

        var em = wind.emission;
        em.enabled = true;
        em.rateOverTime = 0f;

        var r = wind.GetComponent<ParticleSystemRenderer>();
        if (visual)
        {
            r.sortingLayerID = visual.sortingLayerID;
            r.sortingOrder = visual.sortingOrder + 1;
        }

        wind.Play();
    }

    void Update()
    {
        if (loop) loop.Set(IsOn);

        if (visual)
        {
            if (IsOn && onFrames != null && onFrames.Length > 0)
            {
                animTimer += Time.deltaTime * frameRate;
                visual.sprite = onFrames[(int)animTimer % onFrames.Length];
            }
            else if (offSprite)
            {
                animTimer = 0f;
                visual.sprite = offSprite;
            }
        }

        if (wind)
        {
            var em = wind.emission;
            em.rateOverTime = IsOn ? particlesPerSecond : 0f;
        }
    }

    void FixedUpdate()
    {
        if (!IsOn || !liftZone) return;

        Bounds b = liftZone.bounds;
        foreach (var h in Physics2D.OverlapBoxAll(b.center, b.size, 0f))
        {
            var rb = h.attachedRigidbody;
            if (!rb || rb == selfRb || rb.bodyType != RigidbodyType2D.Dynamic) continue;
            if (rb.GetComponent<Fan>()) continue;
            if (!h.GetComponentInParent<PossessionObject>() && !h.GetComponent<Pushable>()) continue;
            if (rb.linearVelocity.y < maxRiseSpeed) rb.AddForce(Vector2.up * lift);
        }
    }
}