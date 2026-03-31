using UnityEngine;

public class ObjectResetter : MonoBehaviour
{
    [SerializeField] private Vector3 startingPosition = Vector3.zero;
    [SerializeField] private Quaternion startingRotation = Quaternion.identity;
    [SerializeField] private Vector3 startingScale = Vector3.one;

    public void ResetTransform()
    {
        transform.localPosition = startingPosition;
        transform.localScale = startingScale;
        transform.localRotation = startingRotation;
    }

    private void OnEnable()
    {
        ResetTransform();
    }

    public void DeactivateObject()
    {
        //remove all attached objects
        PinTriggerCenter pin = transform.parent.GetComponentInChildren<PinTriggerCenter>();
        if (pin != null)
        {
            pin.DetachFromBlock();
        }

        Jetpack jet = transform.parent.GetComponentInChildren<Jetpack>();
        if (jet != null)
        {
            jet.DetachFromBlock();
        }

        Wheel wheel = transform.parent.GetComponentInChildren<Wheel>();
        if (wheel != null)
        {
            wheel.DetachFromBlock();
        }

        //Detach from parent and destroy parent
        Transform parent = transform.parent;
        transform.parent = null;
        Destroy(parent.gameObject);

        //Deactivate
        gameObject.SetActive(false);
    }
}
