using UnityEngine;
using UnityEngine.Audio;

public class AudioController : MonoBehaviour
{
    [SerializeField] private AudioMixer mainMixer;

    public void SetVolume(float sliderVolume)
    {
        mainMixer.SetFloat("MasterVolume", Mathf.Log10(sliderVolume) * 20);
    }
}
    
