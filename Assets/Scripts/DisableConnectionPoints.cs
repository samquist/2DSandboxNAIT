using UnityEngine;

public class DisableConnectionPoints : MonoBehaviour
{
    public bool currectState = true;

    public void ToggleAllPoints()
    {
        currectState = !currectState;
        foreach (ConnectionPoint point in FindObjectsByType<ConnectionPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            point.gameObject.SetActive(currectState);
        }
    }

    public void EnableAllPoints()
    {
        foreach (ConnectionPoint point in FindObjectsByType<ConnectionPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            point.gameObject.SetActive(true);
        }
    }
}
