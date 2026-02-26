using UnityEngine;

public class ConnectionPoint : MonoBehaviour
{
    public ConnectionPoint connectedTo;
    public ConnectionPoint toConnectTo;
    public bool isConnected = false;
    public bool canConnect = true;
    public bool hasAngle = true;
    public float tangentDegrees;
    [SerializeField] private Color regularColor;
    [SerializeField] private Color highlightedColor;

    private void Awake()
    {
        GetComponentInChildren<SpriteRenderer>().color = regularColor;
        if (hasAngle)
            SetRelativeAngle();
    }

    public void Connect()//called by Drag & Pinch
    {
        Snap();

        ConnectObjects();

        LockConnection();
        connectedTo.LockConnection();

    }

    public void ConnectObjects()
    {
        Transform OtherParent = toConnectTo.transform.parent.parent;
        Transform thisParent = transform.parent.parent;
        
        foreach(var ob in OtherParent.GetComponentsInChildren<Collider2D>())
        {
            if (!ob.isTrigger)
            {
                ob.transform.SetParent(thisParent);
            }
        }

        Destroy(OtherParent.gameObject);
    }

    public void LockConnection()
    {
        connectedTo = toConnectTo;
        toConnectTo = null;
        isConnected = true;
        canConnect = false;
    }

    public void Snap()
    {
        if (hasAngle && toConnectTo.hasAngle)
            RotateForSnap();

        Transform parentTransform = gameObject.transform.parent;

        Debug.Log($"Snapping {parentTransform.gameObject.name} to {toConnectTo.transform.parent.gameObject.name}");

        parentTransform.position = new Vector3(parentTransform.position.x + toConnectTo.transform.position.x - transform.position.x, parentTransform.position.y + toConnectTo.transform.position.y - transform.position.y, parentTransform.position.z);
    }

    public void RotateForSnap()
    {
        float otherTangent = toConnectTo.transform.parent.localEulerAngles.z + toConnectTo.transform.parent.parent.localEulerAngles.z + toConnectTo.tangentDegrees;

        //Debug.Log($"Other Tangent: {otherTangent} Other Parent: {toConnectTo.transform.parent.localEulerAngles.z} Other Local Tangent: {toConnectTo.tangentDegrees}");
        //Debug.Log($"This Tangent: {transform.parent.localEulerAngles.z + tangentDegrees} This Parent: {transform.parent.localEulerAngles.z} This Local Tangent: {tangentDegrees}");

        transform.parent.localEulerAngles = new Vector3(0, 0, otherTangent - 180 - tangentDegrees);

        //Debug.Log($"This Tangent: {transform.parent.localEulerAngles.z + tangentDegrees} This Parent: {transform.parent.localEulerAngles.z} This Local Tangent: {tangentDegrees}");
    }

    public float GetAngle(Vector2 p1, Vector2 p2)
    {
        float angle = 0;
        angle = Mathf.Atan2(p2.y - p1.y, p2.x - p1.x) * 180.0f / Mathf.PI;
        return angle;
    }

    public void SetRelativeAngle()
    {
        tangentDegrees = GetAngle(new Vector2(transform.parent.position.x, transform.parent.position.y), new Vector2(transform.position.x, transform.position.y));
    }

    public void HighlightConnection(ConnectionPoint other)
    {
        toConnectTo = other;
        Debug.Log($"Highlighting Connection - {gameObject.name}");
        //highlight the connection point and a line between the two points?
        GetComponentInChildren<SpriteRenderer>().color = highlightedColor;
    }

    public void UnHighlightConnection(ConnectionPoint other)
    {
        toConnectTo = null;
        Debug.Log($"Unhighlighting Connection - {gameObject.name}");
        //unhighlight the connection point and line between the two points?
        GetComponentInChildren<SpriteRenderer>().color = regularColor;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("ConnectionPoint"))
        {
            ConnectionPoint other = collision.GetComponent<ConnectionPoint>();
            if (canConnect && other.canConnect)
                HighlightConnection(other);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("ConnectionPoint"))
        {
            ConnectionPoint other = collision.GetComponent<ConnectionPoint>();
            if (canConnect && other.canConnect)
                UnHighlightConnection(other);
        }
    }
}
