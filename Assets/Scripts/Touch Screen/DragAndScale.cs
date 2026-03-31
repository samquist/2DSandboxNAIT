using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class DragAndScale : MonoBehaviour
{
    [Header("Throw Behaviour")]
    [SerializeField] private float velocitySmoothing = 8f;
    [SerializeField] private float minThrowSpeed = 0.5f;

    [Header("Scale Behaviour")]
    [SerializeField] public float minScale = 0.5f;
    [SerializeField] public float maxScale = 5f;
    [SerializeField] public float minPosition = 0.025f;
    [SerializeField] public float maxPosition = 0.25f;
    [HideInInspector] public float lowestScale, highestScale;

    [Header("Drag & Scale Bools")]
    public bool isDragged;
    public bool isPinched;

    [Header("Locking")]
    public bool isLockedByPin { get; private set; } = false;

    [Header("Rotation Controls")]
    [Tooltip("Rotation speed for two-finger twist gesture")]
    [SerializeField] private float twistSensitivity = 1f;

    [Tooltip("Invert twist direction")]
    [SerializeField] private bool invertTwistDirection = false;

    [Header("PC Keyboard Rotation")]
    [SerializeField] private float rotateSpeedKeyboard = 180f;

    private Camera mainCam;
    private Rigidbody2D rb;
    private Vector3 offset;
    private float grabDepth;
    private Vector3 lastWorldPos;
    private Vector2 smoothedVelocity;
    private List<ConnectionPoint> connectionPoints;

    private void Awake()
    {
        mainCam = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        AddAllConnectionPoints();

        lowestScale = transform.localScale.x;
        highestScale = transform.localScale.x;
    }

    public void AddAllConnectionPoints()
    {
        connectionPoints = new List<ConnectionPoint>();
        ConnectionPoint[] all = GetComponentsInChildren<ConnectionPoint>();
        for (int i = 0; i < all.Length; i++)
        {
            connectionPoints.Add(all[i]);
        }
    }

    public void OnGrabBegin(Vector2 screenPos)
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        grabDepth = Vector3.Dot(transform.position - mainCam.transform.position, mainCam.transform.forward);

        Vector3 pointerWorld = mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, grabDepth));
        offset = transform.position - pointerWorld;

        isDragged = true;
        rb.bodyType = RigidbodyType2D.Kinematic;

        lastWorldPos = transform.position;
        smoothedVelocity = Vector2.zero;
    }

    public void OnGrabUpdate(Vector2 screenPos)
    {
        if (!isDragged) return;

        Vector3 pointerWorld = mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, grabDepth));
        Vector3 targetPos = pointerWorld + offset;
        targetPos.z = transform.position.z;

        transform.position = targetPos;

        Vector3 worldDelta = targetPos - lastWorldPos;
        float dt = Time.deltaTime > 0f ? Time.deltaTime : 0.016f;
        Vector2 rawVelocity = new Vector2(worldDelta.x, worldDelta.y) / dt;
        smoothedVelocity = Vector2.Lerp(smoothedVelocity, rawVelocity, velocitySmoothing * dt);

        lastWorldPos = targetPos;
    }

    public void OnGrabEnd()
    {
        if (!isDragged) return;

        isDragged = false;

        if (isLockedByPin)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        else
        {
            rb.bodyType = RigidbodyType2D.Dynamic;

            if (smoothedVelocity.magnitude > minThrowSpeed)
            {
                rb.linearVelocity = smoothedVelocity;
            }
            else
            {
                bool flag = false;
                for (int i = 0; i < connectionPoints.Count && !flag; i++)
                {
                    if (connectionPoints[i].canConnect && connectionPoints[i].toConnectTo != null)
                    {
                        connectionPoints[i].Connect();
                        AddAllConnectionPoints();
                        flag = true;
                    }
                }

                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    public void OnPinchBegin()
    {
        isPinched = true;
    }

    public void OnPinchUpdate(float deltaDistance, float deltaRotationDegrees)
    {
        if (!isPinched) return;

        // Scale
        float scaleFactor = 1f + deltaDistance * 0.015f;
        if (scaleFactor * highestScale > maxScale)
        {
            scaleFactor = maxScale / highestScale;
        }
        else if (scaleFactor * lowestScale < minScale)
        {
            scaleFactor = minScale / lowestScale;
        }

        Vector3 newScale = transform.localScale * scaleFactor;
        highestScale *= scaleFactor;
        lowestScale *= scaleFactor;

        newScale.x = Mathf.Clamp(newScale.x, minScale, maxScale);
        newScale.y = newScale.x;
        newScale.z = newScale.x;
        transform.localScale = newScale;

        // Rotation
        if (Mathf.Abs(deltaRotationDegrees) > .01f)
        {
            float rotationAmount = deltaRotationDegrees * twistSensitivity;
            if (invertTwistDirection)
            {
                rotationAmount = -rotationAmount;
            }
            transform.Rotate(0f, 0f, rotationAmount, Space.Self);
        }
    }

    public void OnPinchEnd()
    {
        isPinched = false;
    }

    public void LockByPin()
    {
        isLockedByPin = true;
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    public void UnlockFromPin()
    {
        isLockedByPin = false;
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    private void Update()
    {
        if (isDragged && !isLockedByPin)
        {
            if (Input.GetKey(KeyCode.Q))
            {
                transform.Rotate(0f, 0f, rotateSpeedKeyboard * Time.deltaTime, Space.Self);
            }
            if (Input.GetKey(KeyCode.E))
            {
                transform.Rotate(0f, 0f, -rotateSpeedKeyboard * Time.deltaTime, Space.Self);
            }
        }
    }
}