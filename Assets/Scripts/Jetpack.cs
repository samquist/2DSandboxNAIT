using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(AudioSource))]
public class Jetpack : InteractableObject
{
    [Header("Snapping")]
    [SerializeField] private float maxSnapDistance = 1.5f;
    [SerializeField] private LayerMask blockLayer;

    [Header("Thrust")]
    [SerializeField] private float thrustForce = 20f;
    [SerializeField] private Vector2 thrustLocalDirection = Vector2.up;

    [Header("Behavior")]
    [SerializeField] private float placedLocalZ = 0f;

    [Header("Audio & VFX")]
    [SerializeField] private AudioClip attachSound;
    [SerializeField] private AudioClip thrustStartSound;
    [SerializeField] private AudioClip thrustLoopSound;
    [SerializeField] private AudioClip thrustStopSound;
    [SerializeField] private float thrustLoopStartDelay = 1.0f;
    [SerializeField] private ParticleSystem flameEffect;

    [Header("Tap vs Drag detection")]
    [SerializeField] private float maxTapMovementDistance = 0.45f;
    [SerializeField] private float maxTapDuration = 0.28f;

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
    private Rigidbody2D attachedBlockRb;
    private bool isPlaced;
    private bool isThrustActive;
    private DragAndScale lockedBlock;

    private Vector3 grabStartWorldPosition;
    private float grabStartTime;
    private bool mightBeTap = false;

    private float holdTimer;
    private bool isHoldingForRemoval;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private Vector3 lastBlockPosition;
    private float lastBlockRotation;
    private float lastBlockScale;
    private Coroutine loopStartCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        audioSource = GetComponent<AudioSource>();

        originalRotation = transform.localRotation;
        originalScale = transform.localScale;

        if (flameEffect != null)
        {
            flameEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    private void OnEnable()
    {
        rb.bodyType = RigidbodyType2D.Kinematic;

        foreach (var rigBod in GetComponentsInParent<Rigidbody2D>())
        {
            rigBod.bodyType = RigidbodyType2D.Kinematic;
        }

        if (flameEffect != null)
        {
            flameEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    public override void OnGrabBegin()
    {
        isDragging = true;
        rb.linearVelocity = Vector2.zero;

        grabStartWorldPosition = transform.position;
        grabStartTime = Time.time;
        mightBeTap = true;
        isHoldingForRemoval = false;
        holdTimer = 0f;

        if (attachSound != null)
        {
            audioSource.PlayOneShot(attachSound);
        }
    }

    public override void OnGrabUpdate(Vector2 screenPos)
    {
        if (!isDragging) return;

        if (!isPlaced)
        {
            var cam = Camera.main;
            float depth = Vector3.Dot(transform.position - cam.transform.position, cam.transform.forward);
            var worldPos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, depth));

            float dist = Vector3.Distance(worldPos, grabStartWorldPosition);
            if (dist > maxTapMovementDistance * 0.7f)
                mightBeTap = false;

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
                    float t = Mathf.Clamp01((holdTimer - wiggleStartAfter) / (holdToRemoveDuration - wiggleStartAfter));
                    float amp = Mathf.Lerp(0f, maxWiggleAmplitude, t);
                    float speed = Mathf.Lerp(0f, maxWiggleSpeed, t);
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

        if (isPlaced)
        {
            if (mightBeTap && WasQuickTap())
            {
                ToggleThrust();
            }

            if (lockedBlock != null && lockedBlock.isDragged)
            {
                lockedBlock.OnGrabEnd();
            }
        }
        else if (TryGetNearestBlockCenter(out var blockParent, out Vector3 hitCenter))
        {
            AttachToBlock(blockParent, hitCenter);
        }

        mightBeTap = false;
    }

    private bool WasQuickTap()
    {
        if (!mightBeTap) return false;
        float duration = Time.time - grabStartTime;
        float distance = Vector3.Distance(transform.position, grabStartWorldPosition);
        return duration <= maxTapDuration && distance <= maxTapMovementDistance;
    }

    public bool TryGetNearestBlockCenter(out DragAndScale blockParent, out Vector3 hitCenter)
    {
        blockParent = null;
        hitCenter = Vector3.zero;

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
        transform.localRotation = Quaternion.identity;

        isPlaced = true;
        lockedBlock = blockParent;
        attachedBlockRb = blockParent.GetComponent<Rigidbody2D>();

        if (attachSound != null)
        {
            audioSource.PlayOneShot(attachSound);
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
        isThrustActive = false;

        //rb.bodyType = RigidbodyType2D.Kinematic;

        if (lockedBlock != null)
        {
            lockedBlock.UnlockFromPin();
            lockedBlock = null;
        }

        attachedBlockRb = null;

        if (flameEffect != null)
        {
            flameEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (audioSource.isPlaying && audioSource.clip == thrustLoopSound)
        {
            audioSource.Stop();
            audioSource.loop = false;
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
        isThrustActive = false;

        rb.bodyType = RigidbodyType2D.Kinematic;
        attachedBlockRb = null;

        if (flameEffect != null)
        {
            flameEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (audioSource.isPlaying && audioSource.clip == thrustLoopSound)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        isHoldingForRemoval = false;
        holdTimer = 0f;

        OnGrabBegin();
        OnGrabUpdate(screenPos);
    }

    private void FixedUpdate()
    {
        if (!isPlaced || !isThrustActive || attachedBlockRb == null) return;

        Vector2 worldForceDir = transform.TransformDirection(thrustLocalDirection);
        attachedBlockRb.AddForce(worldForceDir * thrustForce, ForceMode2D.Force);
    }

    private void ToggleThrust()
    {
        isThrustActive = !isThrustActive;

        if (isThrustActive)
        {
            if (thrustStartSound != null)
            {
                audioSource.PlayOneShot(thrustStartSound);
            }

            if (thrustLoopSound != null)
            {
                if (loopStartCoroutine != null)
                {
                    StopCoroutine(loopStartCoroutine);
                }

                loopStartCoroutine = StartCoroutine(StartLoopAfterDelay());
            }

            if (flameEffect != null)
            {
                flameEffect.Play();
            }
        }
        else
        {
            if (loopStartCoroutine != null)
            {
                StopCoroutine(loopStartCoroutine);
                loopStartCoroutine = null;
            }

            if (audioSource.isPlaying && audioSource.clip == thrustLoopSound)
            {
                audioSource.Stop();
                audioSource.loop = false;
            }

            if (thrustStopSound != null)
            {
                audioSource.PlayOneShot(thrustStopSound);
            }

            if (flameEffect != null)
            {
                flameEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }

    private IEnumerator StartLoopAfterDelay()
    {
        yield return new WaitForSeconds(thrustLoopStartDelay);

        if (isThrustActive && thrustLoopSound != null)
        {
            audioSource.clip = thrustLoopSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        loopStartCoroutine = null;
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