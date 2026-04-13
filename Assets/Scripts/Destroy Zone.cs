using UnityEngine;

public class DestroyZone : MonoBehaviour
{
    [Header("Reset Settings")]
    [Tooltip("Small delay helps Zibra Liquid stability")]
    public float resetDelay = 0.08f;

    [Tooltip("Zero out velocity when resetting")]
    public bool resetVelocity = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Transform root = other.attachedRigidbody != null ? other.attachedRigidbody.transform : other.transform.root;

        GameObject rootObj = root.gameObject;

        ConnectionPoint[] allConnectionPoints = rootObj.GetComponentsInChildren<ConnectionPoint>(true);

        foreach (var cp in allConnectionPoints)
        {
            if (cp.isConnected)
            {
                cp.DisconnectWholeObject();
            }
            else if (cp.toConnectTo != null)
            {
                cp.UnHighlightConnection();
            }
        }

        ObjectResetter[] resetters = rootObj.GetComponentsInChildren<ObjectResetter>(true);

        if (resetters.Length > 0)
        {
            foreach (var resetter in resetters)
            {
                if (resetter != null)
                {
                    resetter.DeactivateObject();

                    if (resetDelay <= 0f)
                    {
                        RepairAndPool(resetter);
                    }
                    else
                    {
                        Invoke("RepairAndPoolDelayed", resetDelay);
                    }
                }
            }
        }
        else
        {
            DisableAndReset(rootObj);
        }
    }

    private void RepairAndPool(ObjectResetter resetter)
    {
        if (resetter == null) return;

        GameObject innerObj = resetter.gameObject;
        Transform currentParent = innerObj.transform.parent;

        if (currentParent == null || currentParent.GetComponent<DragAndScale>() == null)
        {
            innerObj.transform.SetParent(null);
        }

        resetter.ResetTransform();

        Rigidbody2D rb = innerObj.GetComponent<Rigidbody2D>() ?? innerObj.GetComponentInParent<Rigidbody2D>();
        if (rb != null && resetVelocity)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        innerObj.SetActive(false);
        if (innerObj.transform.parent != null)
        {
            innerObj.transform.parent.gameObject.SetActive(false);
        }
    }

    private void DisableAndReset(GameObject obj)
    {
        if (obj == null) return;

        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null && resetVelocity)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        obj.SetActive(false);
    }
}