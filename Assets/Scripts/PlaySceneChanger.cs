using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayScene : MonoBehaviour
{
    public void GoToScene()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
