using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class TouchDragScaleManager : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private float scrollStepSize = 0.01f;

    private InputAction pointerPosition;
    private InputAction primaryContact;
    private InputAction scrollAction;

    private DragAndScale selectedObject;

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

        pointerPosition = playerMap.FindAction("PointerPosition");
        primaryContact = playerMap.FindAction("PrimaryContact");
        scrollAction = playerMap.FindAction("Scroll");

        pointerPosition?.Enable();
        primaryContact?.Enable();
        scrollAction?.Enable();

        primaryContact.started += OnPrimaryDown;
        primaryContact.canceled += OnPrimaryUp;
        scrollAction.performed += OnScrollPerformed;
    }

    private void OnDisable()
    {
        primaryContact.started -= OnPrimaryDown;
        primaryContact.canceled -= OnPrimaryUp;
        scrollAction.performed -= OnScrollPerformed;

        pointerPosition?.Disable();
        primaryContact?.Disable();
        scrollAction?.Disable();

        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        if (selectedObject == null) return;

        Vector2 screenPos = pointerPosition?.ReadValue<Vector2>() ?? Vector2.zero;

        if (selectedObject.isDragged)
        {
            selectedObject.OnGrabUpdate(screenPos);
        }

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

    private void OnPrimaryDown(InputAction.CallbackContext ctx)
    {
        if (selectedObject != null)
        {
            return;
        }

        Vector2 screenPos = pointerPosition?.ReadValue<Vector2>() ?? Vector2.zero;

        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);

        if (hit.collider != null)
        {
            DragAndScale drag = hit.collider.GetComponent<DragAndScale>();
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
    }

    private void OnPrimaryUp(InputAction.CallbackContext ctx)
    {
        if (selectedObject != null)
        {
            selectedObject.OnGrabEnd();
            selectedObject.OnPinchEnd();
            selectedObject = null;
        }
    }

    private void OnScrollPerformed(InputAction.CallbackContext ctx)
    {
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
            newScale.y = Mathf.Clamp(newScale.y, selectedObject.minScale, selectedObject.maxScale);
            newScale.z = newScale.x;

            selectedObject.transform.localScale = newScale;

            //--- Position ---
            Vector3 currentPosition = selectedObject.transform.localPosition;
            Vector3 newPosition = currentPosition + new Vector3(scaleDelta, scaleDelta, scaleDelta) / 2;

            newPosition.z = Mathf.Clamp(newPosition.z, selectedObject.minPosition, selectedObject.maxPosition);

            //newPosition.z = newPosition.x;
            selectedObject.transform.localPosition = newPosition;

            if (selectedObject.isPinched)
            {
                selectedObject.OnPinchEnd();
            }
        }
    }
}