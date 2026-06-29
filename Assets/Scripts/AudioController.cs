using UnityEngine;
using UnityEngine.Audio;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance;

    [SerializeField] private AudioMixer mainMixer;
    public float volume = 1.0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetVolume(float sliderVolume)
    {
        volume = sliderVolume;
        mainMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
    }
}
    
