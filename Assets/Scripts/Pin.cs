using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))]
public class PinTriggerCenter : InteractableObject
{
    [Header("Snapping")]
    [SerializeField] private float maxSnapDistance = 0f;
    [SerializeField] private LayerMask blockLayer;

    [Header("Behavior")]
    [SerializeField] private float placedLocalZ = 0f;

    [Header("Audio")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip placeSound;
    [SerializeField] private AudioClip popSound;

    [Header("Removal (Long Hold on Block)")]
    [SerializeField] private float holdToRemoveDuration = 1.2f;
    [SerializeField] private float maxPositionDelta = 0.02f;
    [SerializeField] private float maxRotationDelta = 0.5f;
    [SerializeField] private float maxScaleDelta = 0.001f;
    [SerializeField] private float wiggleStartAfter = 0.4f;
    [SerializeField] private float maxWiggleSpeed = 25f;
    [SerializeField] private float maxWiggleAmplitude = 8f;

    [Header("Scaling")]
    [SerializeField] private float scrollStepSize = 0.1f;

    private Rigidbody2D rb;
    private AudioSource audioSource;
    private bool isPlaced;
    public DragAndScale lockedBlock;

    private float holdTimer;
    private bool isHoldingForRemoval;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private Vector3 lastBlockPosition;
    private float lastBlockRotation;
    private float lastBlockScale;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        audioSource = GetComponent<AudioSource>();

        originalRotation = transform.localRotation;
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        rb.bodyType = RigidbodyType2D.Kinematic;
        
        foreach (var rigBod in GetComponentsInParent<Rigidbody2D>())
        {
            rigBod.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    public override void OnGrabBegin()
    {
        isDragging = true;
        rb.linearVelocity = Vector2.zero;

        if (!isPlaced)
        {
            if (clickSound != null)
            {
                audioSource.clip = clickSound;
                audioSource.loop = false;
                audioSource.Play();
            }
            return;
        }

        isHoldingForRemoval = false;
        holdTimer = 0f;
    }

    public override void OnGrabUpdate(Vector2 screenPos)
    {
        if (!isDragging) return;

        if (!isPlaced)
        {
            var cam = Camera.main;
            float depth = Vector3.Dot(transform.position - cam.transform.position, cam.transform.forward);
            var worldPos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, depth));
            transform.position = new Vector3(worldPos.x, worldPos.y, transform.position.z);
        }
        else if (lockedBlock != null)
        {
            if (!lockedBlock.isDragged)
            {
                lockedBlock.OnGrabBegin(screenPos);
                lastBlockPosition = lockedBlock.transform.position;
                lastBlockRotation = lockedBlock.transform.eulerAngles.z;
                lastBlockScale = lockedBlock.transform.localScale.x;
            }
            lockedBlock.OnGrabUpdate(screenPos);

            Vector3 currentBlockPos = lockedBlock.transform.position;
            float positionDelta = Vector3.Distance(currentBlockPos, lastBlockPosition);
            float rotationDelta = Mathf.Abs(Mathf.DeltaAngle(lockedBlock.transform.eulerAngles.z, lastBlockRotation));
            float scaleDelta = Mathf.Abs(lockedBlock.transform.localScale.x - lastBlockScale);

            if (positionDelta > maxPositionDelta || rotationDelta > maxRotationDelta || scaleDelta > maxScaleDelta)
            {
                holdTimer = 0f;
                isHoldingForRemoval = false;
                transform.localRotation = originalRotation;
            }
            else
            {
                if (!isHoldingForRemoval)
                {
                    isHoldingForRemoval = true;
                    holdTimer = 0f;
                }

                holdTimer += Time.deltaTime;

                if (holdTimer >= wiggleStartAfter)
                {
                    float progress = Mathf.Clamp01((holdTimer - wiggleStartAfter) / (holdToRemoveDuration - wiggleStartAfter));
                    float amp = Mathf.Lerp(0f, maxWiggleAmplitude, progress);
                    float speed = Mathf.Lerp(0f, maxWiggleSpeed, progress);
                    float angle = Mathf.Sin(Time.time * speed) * amp;

                    transform.localRotation = originalRotation * Quaternion.Euler(0, 0, angle);
                }

                if (holdTimer >= holdToRemoveDuration)
                {
                    DetachFromBlockButKeepHolding(screenPos);
                    return;
                }
            }

            lastBlockPosition = currentBlockPos;
            lastBlockRotation = lockedBlock.transform.eulerAngles.z;
            lastBlockScale = lockedBlock.transform.localScale.x;
        }
    }

    public override void OnGrabEnd()
    {
        if (!isDragging) return;

        isDragging = false;
        isHoldingForRemoval = false;
        holdTimer = 0f;
        transform.localRotation = originalRotation;

        if (isPlaced && lockedBlock != null && lockedBlock.isDragged)
        {
            lockedBlock.OnGrabEnd();
        }
        else if (!isPlaced && TryGetNearestBlockCenter(out var blockParent, out Vector3 hitCenter))
        {
            AttachToBlock(blockParent, hitCenter);
        }
    }

    public void DetachFromBlock()
    {
        if (!isPlaced) return;

        var worldPosBefore = transform.position;

        transform.SetParent(null, true);
        transform.position = new Vector3(worldPosBefore.x, worldPosBefore.y, worldPosBefore.z);
        transform.localRotation = originalRotation;
        transform.localScale = originalScale;

        isPlaced = false;
        rb.bodyType = RigidbodyType2D.Kinematic;

        if (lockedBlock != null)
        {
            lockedBlock.UnlockFromPin();
            lockedBlock = null;
        }

        if (popSound != null)
        {
            audioSource.PlayOneShot(popSound);
        }
    }

    private void DetachFromBlockButKeepHolding(Vector2 screenPos)
    {
        if (!isPlaced) return;

        if (lockedBlock != null)
        {
            lockedBlock.UnlockFromPin();
            lockedBlock = null;
        }

        var worldPosBefore = transform.position;
        transform.SetParent(null, true);
        transform.position = new Vector3(worldPosBefore.x, worldPosBefore.y, worldPosBefore.z);
        transform.localRotation = originalRotation;
        transform.localScale = originalScale;

        isPlaced = false;
        rb.bodyType = RigidbodyType2D.Kinematic;

        if (popSound != null)
        {
            audioSource.PlayOneShot(popSound);
        }

        isHoldingForRemoval = false;
        holdTimer = 0f;

        OnGrabBegin();
        OnGrabUpdate(screenPos);
    }

    private bool TryGetNearestBlockCenter(out DragAndScale blockParent, out Vector3 hitCenter)
    {
        blockParent = null;
        hitCenter = new Vector3();

        var hits = Physics2D.OverlapCircleAll(transform.position, maxSnapDistance, blockLayer);
        if (hits.Length == 0) return false;

        float bestDistSq = float.MaxValue;

        foreach (var hit in hits)
        {
            var candidate = hit.GetComponentInParent<DragAndScale>();
            if (candidate == null || hit.isTrigger) continue;

            var blockCenter = hit.transform.position;
            var distSq = (transform.position - blockCenter).sqrMagnitude;

            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                blockParent = candidate;
                hitCenter = blockCenter;
            }
        }

        return blockParent != null;
    }

    public void AttachToBlock(DragAndScale blockParent, Vector3 hitCenter)
    {
        transform.SetParent(blockParent.transform, true);
        transform.position = new Vector3(hitCenter.x, hitCenter.y, placedLocalZ);
        transform.localRotation = originalRotation;

        isPlaced = true;
        lockedBlock = blockParent;

        rb.bodyType = RigidbodyType2D.Kinematic;
        blockParent.LockByPin();

        if (placeSound != null)
        {
            audioSource.PlayOneShot(placeSound);
        }
    }

    public override void OnScrollPerformed(InputAction.CallbackContext ctx)
    {
        if (lockedBlock == null || !isPlaced) return;
        lockedBlock.ApplyScrollScaling(ctx, scrollStepSize);
        holdTimer = 0f;
        isHoldingForRemoval = false;
        transform.localRotation = originalRotation;
    }
}