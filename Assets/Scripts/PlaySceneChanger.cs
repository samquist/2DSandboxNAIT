using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayScene : MonoBehaviour
{
    public void GoToLevel1()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void GoToLevel2()
    {
        SceneManager.LoadScene("SCENE2");
    }

    public void GoToLevel3()
    {
        SceneManager.LoadScene("SCENE3");
    }

    public void GoToLevel4()
    {
        SceneManager.LoadScene("SCENE4");
    }
}
