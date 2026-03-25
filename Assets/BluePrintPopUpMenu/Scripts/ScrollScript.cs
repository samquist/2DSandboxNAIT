using UnityEngine;

public class ScrollScript : MonoBehaviour
{
    [Header("Components")]
    public Animator animator;
    public GameObject blueprintPages;

    [Header("Settings")]
    public float openDelay = 0.1f;

    private bool isOpen = false;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();

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
        }
        else
        {
            animator.SetBool("IsOpen", true);
        }

        isOpen = !isOpen;
    }
}
