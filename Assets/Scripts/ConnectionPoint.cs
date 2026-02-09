using UnityEngine;

public class ConnectionPoint : MonoBehaviour
{
    public ConnectionPoint connectedTo;
    public ConnectionPoint toConnectTo;
    public bool isConnected = false;
    public bool canConnect = false;
    [SerializeField] private Color regularColor;
    [SerializeField] private Color highlightedColor;

    private void Awake()
    {
        GetComponentInChildren<SpriteRenderer>().color = regularColor;
    }

    public void ConnectTo(ConnectionPoint other)
    {
    }

    public void HighlightConnection(ConnectionPoint other)
    {
        toConnectTo = other;
        canConnect = true;
        other.toConnectTo = this;
        other.canConnect = true;
        Debug.Log($"Highlighting Connection - {gameObject.name}");
        //highlight the connection point and a line between the two points?
        GetComponentInChildren<SpriteRenderer>().color = highlightedColor;
    }

    public void UnHighlightConnection(ConnectionPoint other)
    {
        toConnectTo = null;
        canConnect = false;
        other.toConnectTo = null;
        other.canConnect = true;
        Debug.Log($"Unhighlighting Connection - {gameObject.name}");
        //unhighlight the connection point and line between the two points?
        GetComponentInChildren<SpriteRenderer>().color = regularColor;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("ConnectionPoint"))
        {
            ConnectionPoint other = collision.GetComponent<ConnectionPoint>();
            HighlightConnection(other);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("ConnectionPoint"))
        {
            ConnectionPoint other = collision.GetComponent<ConnectionPoint>();
            UnHighlightConnection(other);
        }
    }
}
