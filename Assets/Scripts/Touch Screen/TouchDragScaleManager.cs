using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class TouchDragScaleManager : MonoBehaviour
{
    [SerializeField] public InputActionAsset inputActions;
    [SerializeField] private float scrollStepSize = 0.01f;

    private InputAction touchPosition;
    private InputAction mousePosition;
    private InputAction touchContact;
    private InputAction mouseContact;
    private InputAction scrollAction;

    private DragAndScale selectedObject;
    private InteractableObject currentInteractableObject;

    private bool isUsingTouch;

    private Vector2 prevFinger0ScreenPos;
    private Vector2 prevFinger1ScreenPos;
    private bool wasTwoFingersPreviousFrame;

    private void Awake()
    {
        if (inputActions == null)
        {
            Debug.LogError("[TouchDragScaleManager] InputActionAsset not assigned!");
        }
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();

        var playerMap = inputActions?.FindActionMap("Player");
        if (playerMap == null)
        {
            Debug.LogWarning("[TouchDragScaleManager] Player action map not found!");
            return;
        }

        touchPosition = playerMap.FindAction("TouchPosition");
        mousePosition = playerMap.FindAction("MousePosition");
        touchContact = playerMap.FindAction("TouchContact");
        mouseContact = playerMap.FindAction("MouseContact");
        scrollAction = playerMap.FindAction("Scroll");

        touchPosition?.Enable();
        mousePosition?.Enable();
        touchContact?.Enable();
        mouseContact?.Enable();
        scrollAction?.Enable();

        touchContact.started += OnTouchDown;
        touchContact.canceled += OnPrimaryUp;
        mouseContact.started += OnMouseDown;
        mouseContact.canceled += OnPrimaryUp;
        scrollAction.performed += OnScrollPerformed;
    }

    private void OnDisable()
    {
        if (touchContact != null)
        {
            touchContact.started -= OnTouchDown;
            touchContact.canceled -= OnPrimaryUp;
        }
        if (mouseContact != null)
        {
            mouseContact.started -= OnMouseDown;
            mouseContact.canceled -= OnPrimaryUp;
        }
        if (scrollAction != null)
        {
            scrollAction.performed -= OnScrollPerformed;
        }

        touchContact.started -= OnTouchDown;
        touchContact.canceled -= OnPrimaryUp;
        mouseContact.started -= OnMouseDown;
        mouseContact.canceled -= OnPrimaryUp;
        scrollAction.performed -= OnScrollPerformed;

        touchPosition?.Disable();
        mousePosition?.Disable();
        touchContact?.Disable();
        mouseContact?.Disable();
        scrollAction?.Disable();

        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        Vector2 screenPos = isUsingTouch
            ? touchPosition?.ReadValue<Vector2>() ?? Vector2.zero
            : mousePosition?.ReadValue<Vector2>() ?? Vector2.zero;

        if (currentInteractableObject != null && currentInteractableObject.isDragging)
        {
            currentInteractableObject.OnGrabUpdate(screenPos);
        }
        else if (selectedObject != null && selectedObject.isDragged)
        {
            selectedObject.OnGrabUpdate(screenPos);
        }

        if (selectedObject != null)
        {
            if (Touch.activeFingers.Count == 2)
            {
                var finger0 = Touch.activeFingers[0].currentTouch;
                var finger1 = Touch.activeFingers[1].currentTouch;

                Vector2 currPos0 = finger0.screenPosition;
                Vector2 currPos1 = finger1.screenPosition;
                float currentDistance = Vector2.Distance(currPos0, currPos1);

                if (!selectedObject.isPinched)
                {
                    selectedObject.OnPinchBegin();
                }

                float deltaDistance = 0f;
                float deltaRotationDegrees = 0f;

                if (wasTwoFingersPreviousFrame)
                {
                    float prevDistance = Vector2.Distance(prevFinger0ScreenPos, prevFinger1ScreenPos);
                    deltaDistance = currentDistance - prevDistance;

                    Vector2 prevVec = prevFinger1ScreenPos - prevFinger0ScreenPos;
                    Vector2 currVec = currPos1 - currPos0;
                    deltaRotationDegrees = Vector2.SignedAngle(prevVec, currVec);
                }

                selectedObject.OnPinchUpdate(deltaDistance, deltaRotationDegrees);

                prevFinger0ScreenPos = currPos0;
                prevFinger1ScreenPos = currPos1;
                wasTwoFingersPreviousFrame = true;
            }
            else
            {
                wasTwoFingersPreviousFrame = false;
                if (selectedObject.isPinched)
                {
                    selectedObject.OnPinchEnd();
                }
            }
        }

        if (currentInteractableObject != null)
        {
            if (Touch.activeFingers.Count == 2)
            {
                var finger0 = Touch.activeFingers[0].currentTouch;
                var finger1 = Touch.activeFingers[1].currentTouch;

                Vector2 currPos0 = finger0.screenPosition;
                Vector2 currPos1 = finger1.screenPosition;
                float currentDistance = Vector2.Distance(currPos0, currPos1);

                if (!currentInteractableObject.isPinched)
                {
                    currentInteractableObject.OnPinchBegin();
                }

                float deltaDistance = 0f;
                float deltaRotationDegrees = 0f;

                if (wasTwoFingersPreviousFrame)
                {
                    float prevDistance = Vector2.Distance(prevFinger0ScreenPos, prevFinger1ScreenPos);
                    deltaDistance = currentDistance - prevDistance;

                    Vector2 prevVec = prevFinger1ScreenPos - prevFinger0ScreenPos;
                    Vector2 currVec = currPos1 - currPos0;
                    deltaRotationDegrees = Vector2.SignedAngle(prevVec, currVec);
                }

                currentInteractableObject.OnPinchUpdate(deltaDistance, deltaRotationDegrees);

                prevFinger0ScreenPos = currPos0;
                prevFinger1ScreenPos = currPos1;
                wasTwoFingersPreviousFrame = true;
            }
            else
            {
                wasTwoFingersPreviousFrame = false;
                if (currentInteractableObject.isPinched)
                {
                    currentInteractableObject.OnPinchEnd();
                }
            }
        }
    }

    private void OnTouchDown(InputAction.CallbackContext ctx)
    {
        isUsingTouch = true;

        if (selectedObject != null || currentInteractableObject != null) return;

        Vector2 screenPos = touchPosition?.ReadValue<Vector2>() ?? Vector2.zero;
        OnPrimaryDown(screenPos);
    }

    private void OnMouseDown(InputAction.CallbackContext ctx)
    {
        isUsingTouch = false;

        if (selectedObject != null || currentInteractableObject != null) return;

        Vector2 screenPos = mousePosition?.ReadValue<Vector2>() ?? Vector2.zero;
        OnPrimaryDown(screenPos);
    }

    private void OnPrimaryDown(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);

        if (hit.collider == null) return;

        PinTriggerCenter pin = hit.collider.GetComponent<PinTriggerCenter>();
        if (pin != null)
        {
            currentInteractableObject = pin;
            currentInteractableObject.OnGrabBegin();
            return;
        }

        Bomb bomb = hit.collider.GetComponent<Bomb>();
        if (bomb != null)
        {
            currentInteractableObject = bomb;
            currentInteractableObject.OnGrabBegin(screenPos);
            return;
        }

        Wheel wheel = hit.collider.GetComponent<Wheel>();
        if (wheel != null)
        {
            currentInteractableObject = wheel;
            currentInteractableObject.OnGrabBegin();
            return;
        }

        Jetpack jetpack = hit.collider.GetComponent<Jetpack>();
        if (jetpack != null)
        {
            currentInteractableObject = jetpack;
            currentInteractableObject.OnGrabBegin();
            return;
        }

        DragAndScale drag = hit.collider.GetComponent<DragAndScale>();
        if (drag == null)
        {
            drag = hit.collider.GetComponentInParent<DragAndScale>();
        }

        if (drag != null)
        {
            selectedObject = drag;
            selectedObject.OnGrabBegin(screenPos);
        }
    }

    private void OnPrimaryUp(InputAction.CallbackContext ctx)
    {
        if (currentInteractableObject != null)
        {
            currentInteractableObject.OnGrabEnd();
            currentInteractableObject.OnPinchEnd();
            currentInteractableObject = null;
            return;
        }

        if (selectedObject != null)
        {
            selectedObject.OnGrabEnd();
            selectedObject.OnPinchEnd();
            selectedObject = null;
        }
    }

    private void OnScrollPerformed(InputAction.CallbackContext ctx)
    {
        if (selectedObject == null)
        {

            if (currentInteractableObject != null)
            {
                currentInteractableObject.OnScrollPerformed(ctx);
            }
            return;
        }

        Vector2 scrollDelta = ctx.ReadValue<Vector2>();
        float scrollY = scrollDelta.y;

        if (Mathf.Abs(scrollY) <= 0.1f) return;

        float direction = Mathf.Sign(scrollY);
        float scaleDelta = direction * scrollStepSize;

        float scaleFactor = (selectedObject.transform.localScale.x + scaleDelta) / selectedObject.transform.localScale.x;
        if (scaleFactor * selectedObject.highestScale > selectedObject.maxScale)
        {
            scaleFactor = selectedObject.maxScale / selectedObject.highestScale;
        }
        else if (scaleFactor * selectedObject.lowestScale < selectedObject.minScale)
        {
            scaleFactor = selectedObject.minScale / selectedObject.lowestScale;
        }

        Vector3 currentScale = selectedObject.transform.localScale;
        Vector3 newScale = currentScale * scaleFactor;
        selectedObject.highestScale *= scaleFactor;
        selectedObject.lowestScale *= scaleFactor;

        newScale.x = Mathf.Clamp(newScale.x, selectedObject.minScale, selectedObject.maxScale);
        newScale.y = newScale.x;
        newScale.z = newScale.x;

        selectedObject.transform.localScale = newScale;

        Vector3 currentPosition = selectedObject.transform.position;
        Vector3 newPosition = currentPosition + new Vector3(scaleDelta, scaleDelta, 0) / 2f;
        selectedObject.transform.position = newPosition;

        if (selectedObject.isPinched)
        {
            selectedObject.OnPinchEnd();
        }
    }
}