using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class ElevatorDoor : PoweredDevice
{
    public Transform leftDoor;
    public Transform rightDoor;
    public Collider2D blocker;
    public float slideDistance = 0f;
    public float openTime = 0.8f;
    public float loadDelay = 0.4f;

    float openAmount;
    Vector3 leftClosed;
    Vector3 rightClosed;
    bool used;

    public bool IsOpen => IsOn;
    bool FullyOpen => openAmount >= 0.99f;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;

        if (leftDoor) leftClosed = leftDoor.localPosition;
        if (rightDoor) rightClosed = rightDoor.localPosition;

        if (slideDistance <= 0f && leftDoor)
        {
            var sr = leftDoor.GetComponentInChildren<SpriteRenderer>();
            if (sr) slideDistance = sr.bounds.size.x;
        }

        openAmount = IsOpen ? 1f : 0f;
        Apply();
    }

    void Update()
    {
        float target = IsOpen ? 1f : 0f;
        openAmount = Mathf.MoveTowards(openAmount, target, Time.deltaTime / Mathf.Max(0.05f, openTime));
        Apply();
    }

    void Apply()
    {
        float eased = Mathf.SmoothStep(0f, 1f, openAmount);
        if (leftDoor) leftDoor.localPosition = leftClosed + Vector3.left * slideDistance * eased;
        if (rightDoor) rightDoor.localPosition = rightClosed + Vector3.right * slideDistance * eased;
        if (blocker) blocker.enabled = openAmount < 0.5f;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!FullyOpen || used) return;

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