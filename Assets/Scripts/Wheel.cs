using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(WheelJoint2D))]
public class Wheel : InteractableObject
{
    [Header("Snapping")]
    [SerializeField] private float maxSnapDistance = 1.5f;
    [SerializeField] private LayerMask blockLayer;

    private Rigidbody2D rb;
    private WheelJoint2D wheelJoint;
    private CircleCollider2D col;

    private bool isAttached;
    private DragAndScale attachedBlock;
    private List<Collider2D> ignoredColliders = new List<Collider2D>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        wheelJoint = GetComponent<WheelJoint2D>();
        col = GetComponent<CircleCollider2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        wheelJoint.enabled = false;
    }

    public override void OnGrabBegin()
    {
        isDragging = true;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        if (isAttached)
        {
            DetachFromBlock();
        }
        else
        {
            IgnoreAllBlockColliders(true);
        }
    }

    public override void OnGrabUpdate(Vector2 screenPos)
    {
        if (!isDragging) return;

        Camera cam = Camera.main;
        float depth = Vector3.Dot(transform.position - cam.transform.position, cam.transform.forward);
        Vector3 world = cam.ScreenToWorldPoint( new Vector3(screenPos.x, screenPos.y, depth));
        transform.position = new Vector3(world.x, world.y, transform.position.z);
    }

    public override void OnGrabEnd()
    {
        if (!isDragging) return;

        isDragging = false;

        if (TryGetNearestBlock(out DragAndScale block))
        {
            AttachToBlock(block);
        }
        else
        {
            IgnoreAllBlockColliders(false);
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    public override void OnPinchEnd()
    {
    }

    private bool TryGetNearestBlock(out DragAndScale block)
    {
        block = null;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, maxSnapDistance, blockLayer);

        if (hits.Length == 0) return false;

        float bestDistSq = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            if (hit.isTrigger) continue;
            if (hit.GetComponent<Wheel>() != null) continue;

            DragAndScale candidate = hit.GetComponentInParent<DragAndScale>();
            if (candidate == null) continue;

            float distSq = (transform.position - hit.transform.position).sqrMagnitude;
            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                block = candidate;
            }
        }

        return block != null;
    }

    private void AttachToBlock(DragAndScale block)
    {
        Rigidbody2D blockRb = block.GetComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        blockRb.bodyType = RigidbodyType2D.Dynamic;

        IgnoreAllBlockColliders(false);
        IgnoreBlockColliders(block, true);

        wheelJoint.connectedBody = blockRb;
        wheelJoint.connectedAnchor = blockRb.transform.InverseTransformPoint(transform.position);

        wheelJoint.enabled = true;

        isAttached = true;
        attachedBlock = block;
    }

    private void DetachFromBlock()
    {
        if (!isAttached) return;

        wheelJoint.enabled = false;
        wheelJoint.connectedBody = null;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        if (attachedBlock != null)
        {
            IgnoreBlockColliders(attachedBlock, false);
        }

        isAttached = false;
        attachedBlock = null;
    }

    private void IgnoreAllBlockColliders(bool ignore)
    {
        ignoredColliders.Clear();

        foreach (DragAndScale block in FindObjectsByType<DragAndScale>(FindObjectsSortMode.None))
        {
            IgnoreBlockColliders(block, ignore);
        }
    }

    private void IgnoreBlockColliders(DragAndScale block, bool ignore)
    {
        foreach (Collider2D blockCol in block.GetComponentsInChildren<Collider2D>())
        {
            if (blockCol.isTrigger) continue;

            Physics2D.IgnoreCollision(col, blockCol, ignore);

            if (ignore && !ignoredColliders.Contains(blockCol))
            {
                ignoredColliders.Add(blockCol);
            }
            else if (!ignore)
            {
                ignoredColliders.Remove(blockCol);
            }
        }
    }
}