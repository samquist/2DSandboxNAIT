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

    public void Connect()
    {
        Snap();
    }

    public void Snap()
    {
        RotateForSnap();

        Transform parentTransform = gameObject.transform.parent;

        Debug.Log($"Snapping {parentTransform.gameObject.name} to {toConnectTo.transform.parent.gameObject.name}");

        parentTransform.position = new Vector3(parentTransform.position.x + toConnectTo.transform.position.x - transform.position.x, parentTransform.position.y + toConnectTo.transform.position.y - transform.position.y, 0);
    }

    public void RotateForSnap()
    {

    }

    public void HighlightConnection(ConnectionPoint other)
    {
        toConnectTo = other;
        canConnect = true;
        Debug.Log($"Highlighting Connection - {gameObject.name}");
        //highlight the connection point and a line between the two points?
        GetComponentInChildren<SpriteRenderer>().color = highlightedColor;
    }

    public void UnHighlightConnection(ConnectionPoint other)
    {
        toConnectTo = null;
        canConnect = false;
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
