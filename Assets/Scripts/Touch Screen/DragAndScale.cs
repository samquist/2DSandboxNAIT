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

    [Header("Drag & Scale Bools")]
    public bool isDragged;
    public bool isPinched;

    [Header("Locking")]
    public bool isLockedByPin { get; private set; } = false;

    private Camera mainCam;
    private Rigidbody2D rb;
    private Vector3 offset;
    private float grabDepth;
    private Vector3 lastWorldPos;
    private Vector2 smoothedVelocity;
    private float previousPinchDistance;
    private List<ConnectionPoint> connectionPoints;

    private void Awake()
    {
        mainCam = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        AddAllConnectionPoints();
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
        if (!isDragged)
        {
            return;
        }

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
        if (!isDragged)
        {
            return;
        }

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
        previousPinchDistance = -1f;
    }

    public void OnPinchUpdate(float currentPinchDistance)
    {
        if (!isPinched)
        {
            return;
        }

        if (previousPinchDistance > 0f)
        {
            float delta = currentPinchDistance - previousPinchDistance;
            float scaleFactor = 1f + delta * 0.015f;

            Vector3 newScale = transform.localScale * scaleFactor;
            newScale.x = Mathf.Clamp(newScale.x, minScale, maxScale);
            newScale.y = newScale.x;
            newScale.z = newScale.x;
            transform.localScale = newScale;
        }

        previousPinchDistance = currentPinchDistance;
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
            //rb.bodyType = RigidbodyType2D.Kinematic; //Object stays 'Kinematic' when pin is removed     (Behaviour: Stays in place, changes to Dynamic ONLY when grabbed again)
            rb.bodyType = RigidbodyType2D.Dynamic; //Object changes to 'Dynamic' when pin is removed  (Behaviour: Falls immediately)
        }
    }
}








// Trying to start rotating on objects, falling asleep, just copy/pasted 'progress' and undid to working version.



//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.InputSystem;

//[RequireComponent(typeof(Rigidbody2D))]
//public class DragAndScale : MonoBehaviour
//{
//    [Header("Throw Behaviour")]
//    [SerializeField] private float velocitySmoothing = 8f;
//    [SerializeField] private float minThrowSpeed = 0.5f;

//    [Header("Scale Behaviour")]
//    [SerializeField] public float minScale = 0.5f;
//    [SerializeField] public float maxScale = 5f;
//    [SerializeField] public float minPosition = 0.025f;
//    [SerializeField] public float maxPosition = 0.25f;

//    [Header("Drag & Scale Bools")]
//    public bool isDragged;
//    public bool isPinched;

//    [Header("Locking")]
//    public bool isLockedByPin { get; private set; } = false;

//    [Header("Rotation Controls")]
//    [Tooltip("Time finger must be held still to enter rotate mode")]
//    [SerializeField] private float longPressDuration = 0.55f;

//    [Tooltip("Deadzone, how still finger needs to be to trigger rotate")]
//    [SerializeField] private float rotateDeadZonePixels = 20f;

//    [Tooltip("Rotation speed when dragging in rotate mode")]
//    [SerializeField] private float rotateSpeedTouchDrag = 180f;

//    [Tooltip("Rotation speed for two-finger twist gesture")]
//    [SerializeField] private float twistSensitivity = 1.2f;

//    [Tooltip("Invert twist direction")]
//    [SerializeField] private bool invertTwistDirection = false;

//    [Header("PC Keyboard Rotation")]
//    [SerializeField] private KeyCode rotateLeftKey = KeyCode.Q;
//    [SerializeField] private KeyCode rotateRightKey = KeyCode.E;
//    [SerializeField] private float rotateSpeedKeyboard = 180f;

//    private Camera mainCam;
//    private Rigidbody2D rb;
//    private Vector3 offset;
//    private float grabDepth;
//    private Vector3 lastWorldPos;
//    private Vector2 smoothedVelocity;
//    private List<ConnectionPoint> connectionPoints;

//    private bool isRotateMode;
//    private float longPressTimer;
//    private Vector2 longPressStartScreenPos;

//    private void Awake()
//    {
//        mainCam = Camera.main;
//        rb = GetComponent<Rigidbody2D>();
//        AddAllConnectionPoints();
//    }

//    public void AddAllConnectionPoints()
//    {
//        connectionPoints = new List<ConnectionPoint>();
//        ConnectionPoint[] all = GetComponentsInChildren<ConnectionPoint>();
//        for (int i = 0; i < all.Length; i++)
//        {
//            connectionPoints.Add(all[i]);
//        }
//    }

//    public void OnGrabBegin(Vector2 screenPos)
//    {
//        rb.linearVelocity = Vector2.zero;
//        rb.angularVelocity = 0f;

//        grabDepth = Vector3.Dot(transform.position - mainCam.transform.position, mainCam.transform.forward);

//        Vector3 pointerWorld = mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, grabDepth));
//        offset = transform.position - pointerWorld;

//        isDragged = true;
//        rb.bodyType = RigidbodyType2D.Kinematic;

//        lastWorldPos = transform.position;
//        smoothedVelocity = Vector2.zero;

//        isRotateMode = false;
//        longPressTimer = 0f;
//        longPressStartScreenPos = screenPos;
//    }

//    public void OnGrabUpdate(Vector2 screenPos)
//    {
//        if (!isDragged) return;

//        longPressTimer += Time.deltaTime;

//        Vector3 pointerWorld = mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, grabDepth));
//        Vector3 targetPos = pointerWorld + offset;
//        targetPos.z = transform.position.z;

//        if (isRotateMode)
//        {
//            float deltaX = screenPos.x - longPressStartScreenPos.x;
//            float rotationThisFrame = deltaX * rotateSpeedTouchDrag * Time.deltaTime / Screen.width * 100f;
//            transform.Rotate(0f, 0f, -rotationThisFrame, Space.Self);
//        }
//        else
//        {
//            transform.position = targetPos;

//            float screenDistMoved = Vector2.Distance(screenPos, longPressStartScreenPos);
//            if (longPressTimer >= longPressDuration && screenDistMoved < rotateDeadZonePixels)
//            {
//                isRotateMode = true;
//            }

//            Vector3 worldDelta = targetPos - lastWorldPos;
//            float dt = Time.deltaTime > 0f ? Time.deltaTime : 0.016f;
//            Vector2 rawVelocity = new Vector2(worldDelta.x, worldDelta.y) / dt;
//            smoothedVelocity = Vector2.Lerp(smoothedVelocity, rawVelocity, velocitySmoothing * dt);

//            lastWorldPos = targetPos;
//        }
//    }

//    public void OnGrabEnd()
//    {
//        if (!isDragged) return;

//        isDragged = false;
//        isRotateMode = false;
//        longPressTimer = 0f;

//        if (isLockedByPin)
//        {
//            rb.bodyType = RigidbodyType2D.Kinematic;
//            rb.linearVelocity = Vector2.zero;
//            rb.angularVelocity = 0f;
//        }
//        else
//        {
//            rb.bodyType = RigidbodyType2D.Dynamic;

//            if (smoothedVelocity.magnitude > minThrowSpeed)
//            {
//                rb.linearVelocity = smoothedVelocity;
//            }
//            else
//            {
//                bool flag = false;
//                for (int i = 0; i < connectionPoints.Count && !flag; i++)
//                {
//                    if (connectionPoints[i].canConnect && connectionPoints[i].toConnectTo != null)
//                    {
//                        connectionPoints[i].Connect();
//                        AddAllConnectionPoints();
//                        flag = true;
//                    }
//                }

//                rb.linearVelocity = Vector2.zero;
//            }
//        }
//    }

//    public void OnPinchBegin()
//    {
//        isPinched = true;
//    }

//    public void OnPinchUpdate(float deltaDistance, float deltaRotationDegrees)
//    {
//        if (!isPinched) return;

//        float scaleFactor = 1f + deltaDistance * 0.015f;
//        Vector3 newScale = transform.localScale * scaleFactor;
//        newScale.x = Mathf.Clamp(newScale.x, minScale, maxScale);
//        newScale.y = newScale.x;
//        newScale.z = newScale.x;
//        transform.localScale = newScale;

//        if (Mathf.Abs(deltaRotationDegrees) > 1.5f)
//        {
//            float rotationAmount = deltaRotationDegrees * twistSensitivity;
//            if (invertTwistDirection)
//            {
//                rotationAmount = -rotationAmount;
//            }
//            transform.Rotate(0f, 0f, rotationAmount, Space.Self);
//        }
//    }

//    public void OnPinchEnd()
//    {
//        isPinched = false;
//    }

//    public void LockByPin()
//    {
//        isLockedByPin = true;
//        if (rb != null)
//        {
//            rb.bodyType = RigidbodyType2D.Kinematic;
//            rb.linearVelocity = Vector2.zero;
//            rb.angularVelocity = 0f;
//        }
//    }

//    public void UnlockFromPin()
//    {
//        isLockedByPin = false;
//        if (rb != null)
//        {
//            rb.bodyType = RigidbodyType2D.Dynamic;
//        }
//    }

//    private void Update()
//    {
//        if (Input.GetKey(rotateLeftKey))
//        {
//            transform.Rotate(0f, 0f, rotateSpeedKeyboard * Time.deltaTime, Space.Self);
//        }
//        if (Input.GetKey(rotateRightKey))
//        {
//            transform.Rotate(0f, 0f, -rotateSpeedKeyboard * Time.deltaTime, Space.Self);
//        }
//    }
//}