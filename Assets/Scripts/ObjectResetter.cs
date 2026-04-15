using UnityEngine;

public class ObjectResetter : MonoBehaviour
{
    [SerializeField] private Vector3 startingPosition = Vector3.zero;
    [SerializeField] private Quaternion startingRotation = Quaternion.identity;
    [SerializeField] public Vector3 startingScale = Vector3.one;

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
        foreach (PinTriggerCenter pin in transform.parent.GetComponentsInChildren<PinTriggerCenter>())
        {
            pin.DetachFromBlock();
        }
        
        foreach (Jetpack jet in transform.parent.GetComponentsInChildren<Jetpack>())
        {
            jet.DetachFromBlock();
        }

        foreach (Wheel wheel in transform.parent.GetComponentsInChildren<Wheel>())
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
