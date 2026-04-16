using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayScene : MonoBehaviour
{
    public void GoToLevel1()
    {
        SceneManager.LoadScene("BasicLevel");
    }

    public void GoToLevel2()
    {
        SceneManager.LoadScene("PitLevel");
    }

    public void GoToLevel3()
    {
        SceneManager.LoadScene("GorgeLevel");
    }

    public void GoToLevel4()
    {
        SceneManager.LoadScene("PoolLevel");
    }
}
