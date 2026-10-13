using System.Collections.Generic;
using UnityEngine;

public class GhostAfterimage : MonoBehaviour
{
    public SpriteRenderer source;
    public float spawnInterval = 0.05f;
    public float lifetime = 0.35f;
    public float startAlpha = 0.3f;
    public float endScale = 0.85f;
    public float minSpeed = 1.5f;
    public Color tint = new Color(0.7f, 0.9f, 1f);

    class Echo
    {
        public SpriteRenderer sr;
        public Vector3 baseScale;
        public float age;
    }

    Rigidbody2D rb;
    Transform holder;
    float timer;
    readonly List<Echo> active = new List<Echo>();
    readonly Stack<Echo> pool = new Stack<Echo>();

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!source) source = GetComponentInChildren<SpriteRenderer>();
        holder = new GameObject("GhostEchoes").transform;
    }

    void Update()
    {
        for (int i = active.Count - 1; i >= 0; i--)
        {
            Echo e = active[i];
            e.age += Time.deltaTime;
            float t = e.age / lifetime;

            if (t >= 1f)
            {
                e.sr.gameObject.SetActive(false);
                pool.Push(e);
                active.RemoveAt(i);
                continue;
            }

            Color c = tint;
            c.a = startAlpha * (1f - t);
            e.sr.color = c;
            e.sr.transform.localScale = e.baseScale * Mathf.Lerp(1f, endScale, t);
        }

        bool moving = rb.simulated && rb.linearVelocity.magnitude > minSpeed;
        if (!moving)
        {
            timer = 0f;
            return;
        }

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Spawn();
            timer = spawnInterval;
        }
    }

    void Spawn()
    {
        Echo e;
        if (pool.Count > 0)
        {
            e = pool.Pop();
            e.sr.gameObject.SetActive(true);
        }
        else
        {
            var go = new GameObject("Echo");
            go.transform.SetParent(holder, false);
            e = new Echo { sr = go.AddComponent<SpriteRenderer>() };
        }

        Transform t = e.sr.transform;
        t.position = source.transform.position;
        t.rotation = source.transform.rotation;
        e.baseScale = source.transform.lossyScale;
        t.localScale = e.baseScale;

        e.sr.sprite = source.sprite;
        e.sr.flipX = source.flipX;
        e.sr.sharedMaterial = source.sharedMaterial;
        e.sr.sortingLayerID = source.sortingLayerID;
        e.sr.sortingOrder = source.sortingOrder - 1;
        e.age = 0f;
        active.Add(e);
    }
}