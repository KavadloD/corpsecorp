using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    public int loadScene = -1;

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Space)) return;

        int target = loadScene >= 0 ? loadScene : SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(target);
    }
}