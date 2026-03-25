using UnityEngine;

public class MenuPageManager : MonoBehaviour
{
    public GameObject[] pages; 

    private int currentPage = 0;

    public void NextPage()
    {
        pages[currentPage].SetActive(false);
        currentPage = (currentPage + 1) % pages.Length;
        pages[currentPage].SetActive(true);
    }

    public void PreviousPage()
    {
        pages[currentPage].SetActive(false);
        currentPage = (currentPage - 1) % pages.Length;
        pages[currentPage].SetActive(true);
    }
}