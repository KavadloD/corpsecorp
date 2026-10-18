using UnityEngine;

public class Breakable : MonoBehaviour
{
    public void Break()
    {
        AudioController.PlayAt("box_break", transform.position);
        Destroy(gameObject);
    }
}