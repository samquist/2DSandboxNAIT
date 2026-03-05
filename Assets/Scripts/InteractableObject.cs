using UnityEngine;

public abstract class InteractableObject : MonoBehaviour
{
    public virtual bool isDragging { get; protected set; }

    public abstract void OnGrabBegin();
    public abstract void OnGrabUpdate(Vector2 screenPos);
    public abstract void OnGrabEnd();
    public virtual void OnPinchEnd()
    {
        return;
    }
}
