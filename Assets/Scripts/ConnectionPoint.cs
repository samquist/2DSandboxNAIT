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

    public void Connect()
    {
        Snap();

        LockConnection();
        connectedTo.LockConnection();

        DragAndScale thisObject = transform.parent.GetComponent<DragAndScale>();
        bool thisIsSingleBlock = true;
        if (!thisObject.enabled)
        {
            thisObject = transform.parent.parent.GetComponent<DragAndScale>();
            thisIsSingleBlock = false;
        }

        DragAndScale otherObject = connectedTo.transform.parent.GetComponent<DragAndScale>();
        bool otherIsSingleBlock = true;
        if (!otherObject.enabled)
        {
            otherObject = connectedTo.transform.parent.parent.GetComponent<DragAndScale>();
            otherIsSingleBlock = false;
        }

        ConnectObjects(thisObject, thisIsSingleBlock, otherObject, otherIsSingleBlock);
    }

    public void ConnectObjects(MonoBehaviour thisObject, bool thisIsSingleBlock, MonoBehaviour otherObject, bool otherIsSingleBlock)
    {
        if (thisIsSingleBlock && otherIsSingleBlock)
        {
            Debug.Log("Connecting 2 Signle Objects");
            GameObject newParent = new GameObject("ItemParent");
            newParent.transform.position = thisObject.transform.position;

            thisObject.transform.SetParent(newParent.transform);
            otherObject.transform.SetParent(newParent.transform);

            newParent.AddComponent<PolygonCollider2D>();
            newParent.AddComponent<Rigidbody2D>();
            newParent.AddComponent<DragAndScale>();

            //thisObject.GetComponent<DragAndScale>().enabled = false;
            thisObject.GetComponent<Rigidbody2D>().simulated = false;
            thisObject.GetComponent<Collider2D>().enabled = false;

            //otherObject.GetComponent<DragAndScale>().enabled = false;
            otherObject.GetComponent<Rigidbody2D>().simulated = false;
            otherObject.GetComponent<Collider2D>().enabled = false;
            //newParent.GetComponent<DragAndScale>().AddAllConnectionPoints();
        }
        else if (thisIsSingleBlock)
        {
            thisObject.GetComponent<DragAndScale>().enabled = false;
            thisObject.GetComponent<Rigidbody2D>().simulated = false;
            thisObject.GetComponent<Collider2D>().enabled = false;

            thisObject.transform.SetParent(otherObject.transform);
            otherObject.GetComponent<DragAndScale>().AddAllConnectionPoints();
        }
        else if (otherIsSingleBlock)
        {
            otherObject.GetComponent<DragAndScale>().enabled = false;
            otherObject.GetComponent<Rigidbody2D>().simulated = false;
            otherObject.GetComponent<Collider2D>().enabled = false;

            otherObject.transform.SetParent(thisObject.transform);
            thisObject.GetComponent<DragAndScale>().AddAllConnectionPoints();
        }
        else
        {

        }
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
        float otherTangent = toConnectTo.transform.parent.localEulerAngles.z + toConnectTo.tangentDegrees;

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
