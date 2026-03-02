using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))]
public class PinTriggerCenter : MonoBehaviour
{
    [Header("Snapping")]
    [SerializeField] private float maxSnapDistance = 0f;
    [SerializeField] private LayerMask blockLayer;

    [Header("Behavior")]
    [SerializeField] private float placedLocalZ = 0f;

    [Header("Removal (Long Hold)")]
    [SerializeField] private float holdToRemoveDuration = 0f;
    [SerializeField] private float wiggleStartAfter = 0f;
    [SerializeField] private float maxWiggleSpeed = 0f;
    [SerializeField] private float maxWiggleAmplitude = 0f;

    [Header("Audio")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip placeSound;
    [SerializeField] private AudioClip wiggleSound;
    [SerializeField] private AudioClip popSound;

    private Rigidbody2D rb;
    private AudioSource audioSource;
    public bool isDragging { get; private set; }
    private bool isPlaced;
    public DragAndScale lockedBlock;

    private float holdTimer;
    private bool isHoldingForRemoval;
    private Quaternion originalRotation;
    private Vector3 originalScale;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        audioSource = GetComponent<AudioSource>();

        originalRotation = transform.localRotation;
        originalScale = transform.localScale;
    }

    public void OnGrabBegin()
    {
        if (!isPlaced)
        {
            isDragging = true;
            rb.linearVelocity = Vector2.zero;
            
            if (clickSound != null)
            {
                audioSource.clip = clickSound;
                audioSource.loop = false;
                audioSource.Play();
            }
            return;
        }

        isDragging = true;
        isHoldingForRemoval = true;
        holdTimer = 0f;
        transform.localRotation = originalRotation;

        if (wiggleSound != null)
        {
            audioSource.clip = wiggleSound;
            audioSource.loop = false;
            audioSource.Play();
        }
    }

    public void OnGrabUpdate(Vector2 screenPos)
    {
        if (!isDragging)
        {
            return;
        }

        if (!isPlaced)
        {
            var cam = Camera.main;
            float depth = Vector3.Dot(transform.position - cam.transform.position, cam.transform.forward);
            var worldPos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, depth));
            transform.position = new Vector3(worldPos.x, worldPos.y, transform.position.z);
        }
        else if (isHoldingForRemoval)
        {
            holdTimer += Time.deltaTime;

            if (holdTimer >= wiggleStartAfter)
            {
                float t = Mathf.Clamp01(holdTimer / holdToRemoveDuration);
                float amp = Mathf.Lerp(0f, maxWiggleAmplitude, t);
                float speed = Mathf.Lerp(0f, maxWiggleSpeed, t);
                float angle = Mathf.Sin(Time.time * speed) * amp;
                transform.localRotation = originalRotation * Quaternion.Euler(0, 0, angle);
            }

            if (holdTimer >= holdToRemoveDuration)
            {
                DetachFromBlock();
                isHoldingForRemoval = false;
            }
        }
    }

    public void OnGrabEnd()
    {
        if (!isDragging)
        {
            return;
        }

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        isHoldingForRemoval = false;
        holdTimer = 0f;
        transform.localRotation = originalRotation;

        if (!isPlaced && TryGetNearestBlockCenter(out var blockParent, out Vector3 hitCenter))
        {
            AttachToBlock(blockParent, hitCenter);
        }

        isDragging = false;
    }

    private bool TryGetNearestBlockCenter(out DragAndScale blockParent, out Vector3 hitCenter)
    {
        blockParent = null;
        hitCenter = new Vector3();

        var hits = Physics2D.OverlapCircleAll(transform.position, maxSnapDistance, blockLayer);
        if (hits.Length == 0)
        {
            return false;
        }

        float bestDistSq = float.MaxValue;

        foreach (var hit in hits)
        {
            var candidate = hit.GetComponentInParent<DragAndScale>();
            if (candidate == null)
            {
                continue;
            }

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

    public void DetachFromBlock()
    {
        if (!isPlaced)
        {
            return;
        }

        var worldPosBefore = transform.position;

        transform.SetParent(null, true);
        transform.localRotation = originalRotation;

        transform.position = new Vector3(worldPosBefore.x, worldPosBefore.y, worldPosBefore.z);
        transform.localScale = originalScale;

        isPlaced = false;
        rb.bodyType = RigidbodyType2D.Kinematic;

        if (lockedBlock != null)
        {
            lockedBlock.UnlockFromPin();
        }
        lockedBlock = null;

        if (popSound != null)
        {
            audioSource.PlayOneShot(popSound);
        }
    }
}