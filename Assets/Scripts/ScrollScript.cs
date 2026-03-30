using UnityEngine;

public class ScrollScript : MonoBehaviour
{
    [Header("Components")]
    public Animator animator;
    public GameObject blueprintPages;

    [Header("Audio")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    private AudioSource audioSource;

    private bool isOpen = false;

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (blueprintPages != null)
            blueprintPages.SetActive(false);
    }

    public void OnScrollFullyOpened()
    {
        if (blueprintPages != null)
            blueprintPages.SetActive(true);
    }

    public void OnScrollClose()
    {
        if (blueprintPages != null)
            blueprintPages.SetActive(false);
    }

    public void ScrollPressed()
    {
        if (isOpen)
        {
            animator.SetBool("IsOpen", false);
            PlaySound(closeSound);
        }
        else
        {
            animator.SetBool("IsOpen", true);
            PlaySound(openSound);
        }

        isOpen = !isOpen;
    }

    public void PlaySound(AudioClip sound, bool shouldLoop = false)
    {
        audioSource.Stop();
        audioSource.clip = sound;
        audioSource.loop = shouldLoop;
        audioSource.Play();
    }
}
