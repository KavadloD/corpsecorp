using System;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [Serializable]
    public class Sfx
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        public float minDistance = 3f;
        public float maxDistance = 16f;
        public float pitchMin = 1f;
        public float pitchMax = 1f;
    }

    public static AudioController Instance { get; private set; }

    [Header("Music")]
    public AudioClip corpseTheme;
    public AudioClip ghostTheme;
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    public float fadeTime = 0.25f;

    [Header("Sound effects")]
    [Range(0f, 1f)] public float sfxVolume = 1f;
    public Sfx[] effects;

    AudioSource corpseSource;
    AudioSource ghostSource;
    AudioSource sfxSource;
    GhostMovementScript ghost;
    Transform listener;
    float mix = 1f;
    float lastTarget = 1f;
    float syncTimer;
    bool firstFrame = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        corpseSource = MakeSource(corpseTheme, true);
        ghostSource = MakeSource(ghostTheme, true);
        sfxSource = MakeSource(null, false);

        if (corpseTheme) corpseSource.Play();
        if (ghostTheme) ghostSource.Play();
    }

    AudioSource MakeSource(AudioClip clip, bool loop)
    {
        var s = gameObject.AddComponent<AudioSource>();
        s.clip = clip;
        s.loop = loop;
        s.playOnAwake = false;
        s.spatialBlend = 0f;
        return s;
    }

    void Update()
    {
        if (Instance != this) return;

        if (!ghost) ghost = FindFirstObjectByType<GhostMovementScript>();
        if (ghost) lastTarget = ghost.IsPossessing ? 1f : 0f;

        if (firstFrame)
        {
            mix = lastTarget;
            firstFrame = false;
        }

        mix = Mathf.MoveTowards(mix, lastTarget, Time.unscaledDeltaTime / Mathf.Max(0.02f, fadeTime));

        float angle = mix * Mathf.PI * 0.5f;
        corpseSource.volume = Mathf.Sin(angle) * musicVolume;
        ghostSource.volume = Mathf.Cos(angle) * musicVolume;

        syncTimer += Time.unscaledDeltaTime;
        if (syncTimer > 1f)
        {
            syncTimer = 0f;
            KeepInSync();
        }
    }

    void KeepInSync()
    {
        if (!corpseTheme || !ghostTheme) return;
        if (Mathf.Abs(corpseTheme.length - ghostTheme.length) > 0.05f) return;
        if (!corpseSource.isPlaying || !ghostSource.isPlaying) return;

        if (Mathf.Abs(corpseSource.time - ghostSource.time) > 0.06f)
            ghostSource.time = corpseSource.time;
    }

    public static float Attenuation(Vector3 position, float minDistance, float maxDistance)
    {
        if (Instance == null) return 1f;

        if (!Instance.listener)
        {
            var l = FindFirstObjectByType<AudioListener>();
            if (!l) return 1f;
            Instance.listener = l.transform;
        }

        float d = Vector2.Distance(position, Instance.listener.position);
        return Mathf.Clamp01(Mathf.InverseLerp(maxDistance, minDistance, d));
    }

    public static void Play(string name)
    {
        if (Instance != null) Instance.PlayEffect(name, null);
    }

    public static void PlayAt(string name, Vector3 position)
    {
        if (Instance != null) Instance.PlayEffect(name, position);
    }

    void PlayEffect(string name, Vector3? position)
    {
        foreach (var e in effects)
        {
            if (e == null || e.name != name) continue;

            if (!e.clip)
            {
                Debug.LogWarning("AudioController: '" + name + "' has no clip assigned");
                return;
            }

            float vol = e.volume * sfxVolume;
            if (position != null) vol *= Attenuation(position.Value, e.minDistance, e.maxDistance);
            if (vol < 0.001f) return;

            float lo = e.pitchMin > 0f ? e.pitchMin : 1f;
            float hi = e.pitchMax > 0f ? e.pitchMax : lo;
            float pitch = UnityEngine.Random.Range(lo, Mathf.Max(lo, hi));

            if (Mathf.Approximately(pitch, 1f))
            {
                sfxSource.PlayOneShot(e.clip, vol);
                return;
            }

            var go = new GameObject("SFX " + name);
            var s = go.AddComponent<AudioSource>();
            s.clip = e.clip;
            s.volume = vol;
            s.pitch = pitch;
            s.spatialBlend = 0f;
            s.playOnAwake = false;
            s.Play();

            Destroy(go, e.clip.length / Mathf.Max(0.1f, pitch) + 0.1f);
            return;
        }

        Debug.LogWarning("AudioController: no sound named '" + name + "'");
    }
}