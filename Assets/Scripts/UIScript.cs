using UnityEngine;

public class UIScript : MonoBehaviour
{
    public GameObject PauseUI;

    public void PauseGame()
    {
        PauseUI.SetActive(true);
        Time.timeScale = 0.0f;
    }

    public void ResumeGame()
    {
        PauseUI.SetActive(false);
        Time.timeScale = 1.0f;
    }
}
