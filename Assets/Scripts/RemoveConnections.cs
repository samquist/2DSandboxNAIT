using UnityEngine;

public class RemoveConnections : MonoBehaviour
{
    [SerializeField] private GameObject removeconnectionsPanel;

    private void Awake()
    {
        removeconnectionsPanel.SetActive(false);
    }

    public void RemoveConnectionsButton()
    {
        removeconnectionsPanel.SetActive(true);
    }

    public void ConfirmButton()
    {
        RemoveAllConnections();

        removeconnectionsPanel.SetActive(false);
    }

    public void CancelButton()
    {
        removeconnectionsPanel.SetActive(false);
    }

    public void RemoveAllConnections()
    {
        ConnectionPoint[] allPoints = FindObjectsByType<ConnectionPoint>(FindObjectsSortMode.None);

        foreach (ConnectionPoint point in allPoints)
        {
            if (point.isConnected)
            {
                point.DisconnectWholeObject();
            }
        }
    }
}
