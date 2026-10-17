using UnityEngine;

[DefaultExecutionOrder(200)]
public class ParallaxLayer : MonoBehaviour
{
    [Range(0f, 1f)] public float followFactor = 0.9f;
    public bool lockY;

    Transform cam;
    Vector3 startPos;
    Vector3 camStart;
    bool ready;

    void LateUpdate()
    {
        if (!cam)
        {
            var c = Camera.main;
            if (!c) return;
            cam = c.transform;
        }

        if (!ready)
        {
            startPos = transform.position;
            camStart = cam.position;
            ready = true;
            return;
        }

        Vector3 d = cam.position - camStart;
        float y = lockY ? startPos.y : startPos.y + d.y * followFactor;
        transform.position = new Vector3(startPos.x + d.x * followFactor, y, startPos.z);
    }
}