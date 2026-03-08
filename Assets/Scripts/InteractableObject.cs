using UnityEngine;

public abstract class InteractableObject : MonoBehaviour
{
    public virtual bool isDragging { get; protected set; }

    public virtual void OnGrabBegin()
    {
        return;
    }

    public virtual void OnGrabBegin(Vector2 screenPos)
    {
        return;
    }

    public abstract void OnGrabUpdate(Vector2 screenPos);
    public abstract void OnGrabEnd();
    public virtual void OnPinchEnd()
    {
        return;
    }
}
