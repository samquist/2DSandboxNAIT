using UnityEngine;
using UnityEngine.InputSystem;

public abstract class InteractableObject : MonoBehaviour
{
    public virtual bool isDragging { get; protected set; }
    public virtual bool isPinched { get; protected set; }

    public virtual void OnGrabBegin() { return; }
    public virtual void OnGrabBegin(Vector2 screenPos) { return; }
    public abstract void OnGrabUpdate(Vector2 screenPos);
    public abstract void OnGrabEnd();

    public virtual void OnPinchBegin() { return; }
    public virtual void OnPinchUpdate(float deltaDistance, float deltaRotationDegrees) { return; }
    public virtual void OnPinchEnd() { return; }

    public virtual void OnScrollPerformed(InputAction.CallbackContext ctx) { return; }
}
