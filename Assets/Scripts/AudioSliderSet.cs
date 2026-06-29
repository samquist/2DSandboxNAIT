using UnityEngine;
using UnityEngine.UI;

public class AudioSliderSet : MonoBehaviour
{
    void Start()
    {
        if (AudioController.Instance != null)
            gameObject.GetComponent<Slider>().value = AudioController.Instance.volume;
    }
}
