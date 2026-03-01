using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class TouchDragScaleManager : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private float scrollStepSize = 0.01f;

    private InputAction touchPosition;
    private InputAction mousePosition;
    private InputAction touchContact;
    private InputAction mouseContact;
    private InputAction scrollAction;

    private DragAndScale selectedObject;
    private PinTriggerCenter currentPin;

    private bool isUsingTouch;

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
        Vector2 screenPos;
        if (isUsingTouch)
        {
            screenPos = touchPosition?.ReadValue<Vector2>() ?? Vector2.zero;
        }
        else
        {
            screenPos = mousePosition?.ReadValue<Vector2>() ?? Vector2.zero;
        }

        if (currentPin != null && currentPin.isDragging)
        {
            currentPin.OnGrabUpdate(screenPos);
        }
        else if (selectedObject != null && selectedObject.isDragged)
        {
            selectedObject.OnGrabUpdate(screenPos);
        }

        if (selectedObject != null)
        {
            if (Touch.activeFingers.Count == 2)
            {
                var touch0 = Touch.activeFingers[0].currentTouch;
                var touch1 = Touch.activeFingers[1].currentTouch;
                float currentDist = Vector2.Distance(touch0.screenPosition, touch1.screenPosition);

                if (!selectedObject.isPinched)
                {
                    selectedObject.OnPinchBegin();
                }
                selectedObject.OnPinchUpdate(currentDist);
            }
            else if (selectedObject.isPinched)
            {
                selectedObject.OnPinchEnd();
            }
        }
    }

    private void HandlePointerDown(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);

        if (hit.collider == null) return;

        PinTriggerCenter pin = hit.collider.GetComponent<PinTriggerCenter>();
        if (pin != null)
        {
            currentPin = pin;
            currentPin.OnGrabBegin();
            return;
        }

        DragAndScale drag = hit.collider.GetComponentInParent<DragAndScale>();
        if (drag != null)
        {
            selectedObject = drag;
            selectedObject.OnGrabBegin(screenPos);

            if (Touch.activeFingers.Count == 2)
            {
                var t0 = Touch.activeFingers[0].currentTouch;
                var t1 = Touch.activeFingers[1].currentTouch;
                float dist = Vector2.Distance(t0.screenPosition, t1.screenPosition);
                selectedObject.OnPinchBegin();
                selectedObject.OnPinchUpdate(dist);
            }
        }
    }

    private void OnTouchDown(InputAction.CallbackContext ctx)
    {
        isUsingTouch = true;

        if (selectedObject != null || currentPin != null) return;

        Vector2 screenPos = touchPosition?.ReadValue<Vector2>() ?? Vector2.zero;
        HandlePointerDown(screenPos);
    }
    
    private void OnMouseDown(InputAction.CallbackContext ctx)
    {
        isUsingTouch = false;

        if (selectedObject != null || currentPin != null) return;

        Vector2 screenPos = mousePosition?.ReadValue<Vector2>() ?? Vector2.zero;
        HandlePointerDown(screenPos);
    }

    private void OnPrimaryUp(InputAction.CallbackContext ctx)
    {
        if (currentPin != null)
        {
            currentPin.OnGrabEnd();
            currentPin = null;
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
        if (selectedObject == null) return;

        Vector2 scrollDelta = ctx.ReadValue<Vector2>();
        float scrollY = scrollDelta.y;

        if (Mathf.Abs(scrollY) > 0.1f)
        {
            //float direction = -Mathf.Sign(scrollY); //Uncomment line to swap scroll scale direction
            float direction = Mathf.Sign(scrollY);    //Comment line to ^
            float scaleDelta = direction * scrollStepSize;

            Vector3 currentScale = selectedObject.transform.localScale;
            Vector3 newScale = currentScale + new Vector3(scaleDelta, scaleDelta, scaleDelta);

            newScale.x = Mathf.Clamp(newScale.x, selectedObject.minScale, selectedObject.maxScale);
            newScale.y = newScale.x;
            newScale.z = newScale.x;

            selectedObject.transform.localScale = newScale;

            Vector3 currentPosition = selectedObject.transform.localPosition;
            Vector3 newPosition = currentPosition + new Vector3(scaleDelta, scaleDelta, scaleDelta) / 2;
            newPosition.z = Mathf.Clamp(newPosition.z, selectedObject.minPosition, selectedObject.maxPosition);
            selectedObject.transform.localPosition = newPosition;

            if (selectedObject.isPinched)
            {
                selectedObject.OnPinchEnd();
            }
        }
    }
}