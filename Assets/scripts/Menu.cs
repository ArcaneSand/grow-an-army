using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SoundManager.Instance.PlayTitleLoop();
    }

    public void LoadScene()
    {
        SceneManager.LoadScene("MainScene");
    }
}
