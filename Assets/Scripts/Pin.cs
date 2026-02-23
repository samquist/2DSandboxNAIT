using UnityEngine;

[RequireComponent(typeof(Collider2D))]
//[RequireComponent(typeof(Rigidbody2D))]
//[RequireComponent(typeof(AudioSource))]
public class PinTriggerCenter : MonoBehaviour
{
    [Header("Snapping")]
    [SerializeField] private float maxSnapDistance = 0f;
    [SerializeField] private LayerMask blockLayer;

    [Header("Behavior")]
    [SerializeField] private float placedLocalZ = 0f; // Prevents Pin Z Axis from centering on block

    [Header("Removal (Long Hold)")]
    [SerializeField] private float holdToRemoveDuration = 0f;
    [SerializeField] private float wiggleStartAfter = 0f;
    [SerializeField] private float maxWiggleSpeed = 0f;
    [SerializeField] private float maxWiggleAmplitude = 0f;

    [Header("Audio")]
    //[SerializeField] private AudioClip holdSound;
    //[SerializeField] private AudioClip popSound;

    private Rigidbody2D rb;
    //private AudioSource audioSource;
    public bool isDragging { get; private set; }
    private bool isPlaced;
    private DragAndScale lockedBlock;

    private float holdTimer;
    private bool isHoldingForRemoval;
    private Quaternion originalRotation;
    private Vector3 originalScale;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        //audioSource = GetComponent<AudioSource>();

        originalRotation = transform.localRotation;
        originalScale = transform.localScale;
    }

    public void OnGrabBegin()
    {
        if (!isPlaced)
        {
            isDragging = true;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        isDragging = true;
        isHoldingForRemoval = true;
        holdTimer = 0f;
        transform.localRotation = originalRotation;

        //if (holdSound != null)
        //{
        //    audioSource.clip = holdSound;
        //    audioSource.loop = true;
        //    audioSource.Play();
        //}
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

        //if (audioSource.isPlaying)
        //{
        //    audioSource.Stop();
        //}

        isHoldingForRemoval = false;
        holdTimer = 0f;
        transform.localRotation = originalRotation;

        if (!isPlaced && TryGetNearestBlockCenter(out var block, out _))
        {
            AttachToBlock(block);
        }

        isDragging = false;
    }

    private bool TryGetNearestBlockCenter(out DragAndScale block, out Vector3 centerPos)
    {
        block = null;
        centerPos = Vector3.zero;

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

            var blockCenter = candidate.transform.position;
            var distSq = (transform.position - blockCenter).sqrMagnitude;

            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                block = candidate;
                centerPos = blockCenter;
            }
        }

        return block != null;
    }

    private void AttachToBlock(DragAndScale block)
    {
        transform.SetParent(block.transform, true);
        transform.localPosition = new Vector3(0f, 0f, placedLocalZ);
        transform.localRotation = originalRotation;

        isPlaced = true;
        lockedBlock = block;

        rb.bodyType = RigidbodyType2D.Kinematic;
        block.LockByPin();
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

        transform.position = new Vector3(worldPosBefore.x, worldPosBefore.y, 0f);
        transform.localScale = originalScale;

        isPlaced = false;
        rb.bodyType = RigidbodyType2D.Kinematic;

        if (lockedBlock != null)
        {
            lockedBlock.UnlockFromPin();
        }
        lockedBlock = null;

        //if (popSound != null)
        //{
        //    audioSource.PlayOneShot(popSound);
        //}
    }
}