using UnityEngine;

public class AudioOnCollision : MonoBehaviour
{
    AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (audioSource != null)
        {
             audioSource.PlayOneShot(audioSource.clip);
        }
    }
}
