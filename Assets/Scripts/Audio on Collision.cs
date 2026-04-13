using UnityEngine;
using UnityEngine.Audio;

public class AudioOnCollision : MonoBehaviour
{
    [Header("Impact Sound")]
    [SerializeField] private AudioClip impactClip;

    [Header("Audio Mixer Group")]
    public AudioMixerGroup outputGroup;

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float maxVolume = 1f;
    public float velocityThreshold = 0.5f;
    public float maxVelocityForFullVolume = 25f;

    [Header("Anti-Spam Settings")]
    public float cooldownTime = 0.15f;

    private AudioSource audioSource;
    private float lastPlayTime = 0f;

    public AudioClip ImpactClip
    {
        get => impactClip;
        set
        {
            impactClip = value;
            if (audioSource != null && value != null)
            {
                audioSource.clip = value;
            }
        }
    }

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
        }
    }

    void Start()
    {
        if (audioSource != null && impactClip == null && audioSource.clip != null)
        {
            impactClip = audioSource.clip;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (audioSource == null || impactClip == null)
            return;

        if (Time.time - lastPlayTime < cooldownTime)
            return;

        float impactSpeed = collision.relativeVelocity.magnitude;
        if (impactSpeed < velocityThreshold)
            return;

        float volume = Mathf.Clamp01(impactSpeed / maxVelocityForFullVolume) * maxVolume;

        audioSource.PlayOneShot(impactClip, volume);

        lastPlayTime = Time.time;
    }
}