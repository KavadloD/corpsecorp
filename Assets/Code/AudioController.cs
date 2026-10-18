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
            if (e == null || !e.clip || e.name != name) continue;

            if (position == null)
            {
                sfxSource.PlayOneShot(e.clip, e.volume * sfxVolume);
                return;
            }

            var go = new GameObject("SFX " + name);
            go.transform.position = position.Value;

            var s = go.AddComponent<AudioSource>();
            s.clip = e.clip;
            s.volume = e.volume * sfxVolume;
            s.spatialBlend = 1f;
            s.rolloffMode = AudioRolloffMode.Linear;
            s.minDistance = e.minDistance;
            s.maxDistance = e.maxDistance;
            s.dopplerLevel = 0f;
            s.Play();

            Destroy(go, e.clip.length + 0.1f);
            return;
        }
    }
}