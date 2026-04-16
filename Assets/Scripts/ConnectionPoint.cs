using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class ConnectionPoint : MonoBehaviour
{
    public ConnectionPoint connectedTo;
    public ConnectionPoint toConnectTo;
    public bool isConnected = false;
    public bool canConnect = true;
    public bool hasAngle = true;
    public float tangentDegrees = -1;
    [SerializeField] private Color regularColor;
    [SerializeField] private Color highlightedColor;
    [SerializeField] private GameObject connectionLinePrefab;
    [SerializeField] private GameObject prefabParent;
    private GameObject connectionLine;
    private SpawnManager spawnManager;

    private void Awake()
    {
        GetComponentInChildren<SpriteRenderer>().color = regularColor;
        if (hasAngle && tangentDegrees == -1)
            SetRelativeAngle();

        spawnManager = FindAnyObjectByType<SpawnManager>();
    }

    public void Connect()//called by Drag & Scale
    {
        Snap();

        ConnectObjects();

        LockConnection();
        connectedTo.LockConnection();

        CheckForAttachedObjects();

        Destroy(connectionLine);
    }

    public void DisconnectWholeObject()
    {
        if (!isConnected) return;

        Transform originalParent = transform.parent.parent;

        PinTriggerCenter[] pins = originalParent.GetComponentsInChildren<PinTriggerCenter>();
        Jetpack[] jets = originalParent.GetComponentsInChildren<Jetpack>();
        Wheel[] wheels = originalParent.GetComponentsInChildren<Wheel>();

        foreach (ConnectionPoint c in originalParent.GetComponentsInChildren<ConnectionPoint>(true))
        {
            if (c.isConnected)
                c.UnlockConnection();
        }

        ObjectResetter[] objs = originalParent.GetComponentsInChildren<ObjectResetter>();

        foreach (ObjectResetter obj in objs)
        {
            GameObject fullObj = Instantiate(prefabParent);
            fullObj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y, 0);
            fullObj.transform.localScale = new Vector3(obj.transform.lossyScale.x / obj.startingScale.x, obj.transform.lossyScale.y / obj.startingScale.y, obj.transform.lossyScale.z / obj.startingScale.z);
            obj.transform.SetParent(fullObj.transform);

            if (obj.GetComponent<InteractableObject>() == null) spawnManager.SetObjectParameters(fullObj, obj.GetComponent<Renderer>().sharedMaterial);
            fullObj.SetActive(true);
        }

        foreach (Jetpack jet in jets)
        {
            if (jet.TryGetNearestBlockCenter(out var blockParent, out Vector3 hitCenter))
                jet.AttachToBlock(blockParent, hitCenter);
        }
        foreach (Wheel wheel in wheels)
        {
            if (wheel.TryGetNearestBlock(out DragAndScale blockParent))
                wheel.AttachToBlock(blockParent);
        }
        foreach (PinTriggerCenter pin in pins)
        {
            if (pin.TryGetNearestBlockCenter(out var blockParent, out Vector3 hitCenter))
                pin.AttachToBlock(blockParent, hitCenter);
        }

        Destroy(originalParent.gameObject);
    }

    public async void CheckForAttachedObjects()
    {
        await Task.Delay(1000 / 60);
        Transform originalParent = transform.parent.parent;
        PinTriggerCenter[] pins = originalParent.GetComponentsInChildren<PinTriggerCenter>();
        Jetpack[] jets = originalParent.GetComponentsInChildren<Jetpack>();
        Wheel[] wheels = originalParent.GetComponentsInChildren<Wheel>();

        foreach (Jetpack jet in jets)
        {
            jet.DetachFromBlock();
            if (jet.TryGetNearestBlockCenter(out var blockParent, out Vector3 hitCenter))
                jet.AttachToBlock(blockParent, hitCenter);
        }
        foreach (Wheel wheel in wheels)
        {
            wheel.DetachFromBlock();
            if (wheel.TryGetNearestBlock(out DragAndScale blockParent))
                wheel.AttachToBlock(blockParent);
        }
        foreach (PinTriggerCenter pin in pins)
        {
            pin.DetachFromBlock();
            if (pin.TryGetNearestBlockCenter(out var blockParent, out Vector3 hitCenter))
                pin.AttachToBlock(blockParent, hitCenter);
        }
    }

    public void ConnectObjects()//set all objects with a non-trigger collider inside the other object to be parented to this object
    {
        Transform otherParent = toConnectTo.transform.parent.parent;
        Transform thisParent = transform.parent.parent;
        DragAndScale thisDrag = thisParent.GetComponent<DragAndScale>();
        DragAndScale otherDrag = otherParent.GetComponent<DragAndScale>();

        if (thisDrag != null && otherDrag != null)
        {
            thisDrag.lowestScale = Mathf.Min(thisDrag.lowestScale, otherDrag.lowestScale);
            thisDrag.highestScale = Mathf.Max(thisDrag.highestScale, otherDrag.highestScale);
            thisDrag.GetComponent<Rigidbody2D>().mass += otherDrag.GetComponent<Rigidbody2D>().mass;

            foreach (var ob in otherParent.GetComponentsInChildren<Collider2D>())
            {
                if (!ob.isTrigger && !ob.gameObject.CompareTag("DontMoveOnConnect"))
                {
                    ob.transform.SetParent(thisParent);
                }
            }
        }

        Destroy(otherParent.gameObject);
    }

    public void LockConnection()
    {
        connectedTo = toConnectTo;
        toConnectTo = null;
        isConnected = true;
        canConnect = false;
    }

    public void UnlockConnection()
    {
        connectedTo = null;
        toConnectTo = null;
        isConnected = false;
        canConnect = true;
        UnHighlightConnection();
    }

    public void Snap()
    {
        if (hasAngle && toConnectTo.hasAngle)
            RotateForSnap();

        Transform parentTransform = gameObject.transform.parent.parent;

        Debug.Log($"Snapping {parentTransform.gameObject.name} to {toConnectTo.transform.parent.gameObject.name}");

        parentTransform.position = new Vector3(parentTransform.position.x + toConnectTo.transform.position.x - transform.position.x, parentTransform.position.y + toConnectTo.transform.position.y - transform.position.y, parentTransform.position.z);
    }

    public void RotateForSnap()
    {
        float otherTangent = toConnectTo.transform.parent.eulerAngles.z + toConnectTo.tangentDegrees;

        //Debug.Log($"Other Tangent: {otherTangent} Other Parent: {toConnectTo.transform.parent.localEulerAngles.z} Other Local Tangent: {toConnectTo.tangentDegrees}");
        //Debug.Log($"This Tangent: {transform.parent.localEulerAngles.z + tangentDegrees} This Parent: {transform.parent.localEulerAngles.z} This Local Tangent: {tangentDegrees}");

        transform.parent.parent.localEulerAngles = new Vector3(0, 0, otherTangent - 180 - tangentDegrees - transform.parent.localEulerAngles.z);

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
        UnHighlightAllConnections();
        toConnectTo = other;
        //highlight the connection point and a line between the two points?
        GetComponentInChildren<SpriteRenderer>().color = highlightedColor;
        if (transform.parent.parent.GetComponent<DragAndScale>().isDragged)
        {
            connectionLine = Instantiate(connectionLinePrefab);
            connectionLine.GetComponent<ConnectionLine>().Initialize(transform, other.transform);
        }
    }

    public void UnHighlightConnection()
    {
        toConnectTo = null;
        //unhighlight the connection point and line between the two points?
        GetComponentInChildren<SpriteRenderer>().color = regularColor;
        Destroy(connectionLine);
    }

    public void UnHighlightAllConnections()
    {
        foreach(ConnectionPoint point in transform.parent.parent.GetComponentsInChildren<ConnectionPoint>())
        {
            if (!point.isConnected && point.toConnectTo != null)
            {
                point.toConnectTo.UnHighlightConnection();
                point.UnHighlightConnection();
            }
        }
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
                UnHighlightConnection();
        }
    }
}