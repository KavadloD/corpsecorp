using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class ElevatorDoor : PoweredDevice
{
    public SpriteRenderer visual;
    public Color closedColor = new Color(0.55f, 0.2f, 0.2f);
    public Color openColor = new Color(0.3f, 0.8f, 0.4f);
    public float loadDelay = 0.4f;

    bool used;

    public bool IsOpen => IsOn;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void Update()
    {
        if (visual) visual.color = IsOpen ? openColor : closedColor;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!IsOpen || used) return;

        var corpse = other.GetComponentInParent<PossessionObject>();
        if (!corpse || !corpse.IsPossessed) return;

        used = true;
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log("Last level reached. The ending scene goes here.");
            return;
        }
        Invoke(nameof(LoadNext), loadDelay);
    }

    void LoadNext()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}