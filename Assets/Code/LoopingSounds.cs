using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LoopingSound : MonoBehaviour
{
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 0.6f;
    public bool positional = true;
    public float minDistance = 3f;
    public float maxDistance = 14f;
    public float fadeTime = 0.2f;

    AudioSource src;
    bool wanted;
    float level;

    void Awake()
    {
        src = GetComponent<AudioSource>();
        src.clip = clip;
        src.loop = true;
        src.playOnAwake = false;
        src.spatialBlend = positional ? 1f : 0f;
        src.rolloffMode = AudioRolloffMode.Linear;
        src.minDistance = minDistance;
        src.maxDistance = maxDistance;
        src.dopplerLevel = 0f;
        src.volume = 0f;
    }

    public void Set(bool on)
    {
        wanted = on;
    }

    void Update()
    {
        level = Mathf.MoveTowards(level, wanted ? 1f : 0f, Time.unscaledDeltaTime / Mathf.Max(0.02f, fadeTime));

        float master = AudioController.Instance ? AudioController.Instance.sfxVolume : 1f;
        src.volume = level * volume * master;

        if (level > 0f && !src.isPlaying) src.Play();
        else if (level <= 0f && src.isPlaying) src.Stop();
    }
}