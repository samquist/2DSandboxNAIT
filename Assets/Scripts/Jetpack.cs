using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
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

    [Header("Rotation (Scroll Wheel)")]
    [SerializeField] private float scrollRotationDegreesPerStep = 15f;

    [Header("Audio & VFX")]
    [SerializeField] private AudioClip attachSound;
    [SerializeField] private AudioClip thrustStartSound;
    [SerializeField] private AudioClip thrustStopSound;
    [SerializeField] private ParticleSystem flameEffect;

    private Rigidbody2D rb;
    private AudioSource audioSource;
    private Rigidbody2D attachedBlockRb;
    private bool isPlaced;
    private bool isThrustActive;
    private DragAndScale lockedBlock;
    private InputAction scrollAction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        audioSource = GetComponent<AudioSource>();

        if (flameEffect != null)
            flameEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        var manager = FindFirstObjectByType<TouchDragScaleManager>();
        if (manager != null && manager.inputActions != null)
        {
            var playerMap = manager.inputActions.FindActionMap("Player");
            if (playerMap != null)
            {
                scrollAction = playerMap.FindAction("Scroll");
            }
        }
    }

    private void OnEnable()
    {
        if (scrollAction != null)
        {
            scrollAction.performed += OnScrollPerformed;
            scrollAction.Enable();
        }
    }

    private void OnDisable()
    {
        if (scrollAction != null)
        {
            scrollAction.performed -= OnScrollPerformed;
            scrollAction.Disable();
        }
    }

    private void OnScrollPerformed(InputAction.CallbackContext ctx)
    {
        if (!isDragging && !isPlaced) return;

        Vector2 scrollDelta = ctx.ReadValue<Vector2>();
        float scrollY = scrollDelta.y;

        if (Mathf.Abs(scrollY) < 0.1f) return;

        float rotationThisStep = Mathf.Sign(scrollY) * scrollRotationDegreesPerStep;
        transform.Rotate(0f, 0f, rotationThisStep, Space.Self);
    }

    public override void OnGrabBegin()
    {
        if (isPlaced)
        {
            ToggleThrust();
            return;
        }

        isDragging = true;
        rb.linearVelocity = Vector2.zero;

        if (attachSound != null)
        {
            audioSource.PlayOneShot(attachSound);
        }
    }

    public override void OnGrabUpdate(Vector2 screenPos)
    {
        if (!isDragging || isPlaced) return;

        var cam = Camera.main;
        float depth = Vector3.Dot(transform.position - cam.transform.position, cam.transform.forward);
        var worldPos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, depth));
        transform.position = new Vector3(worldPos.x, worldPos.y, transform.position.z);
    }

    public override void OnGrabEnd()
    {
        if (!isDragging) return;

        isDragging = false;

        if (!isPlaced && TryGetNearestBlockCenter(out var blockParent, out Vector3 hitCenter))
        {
            AttachToBlock(blockParent, hitCenter);
        }
    }

    private bool TryGetNearestBlockCenter(out DragAndScale blockParent, out Vector3 hitCenter)
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

    private void AttachToBlock(DragAndScale blockParent, Vector3 hitCenter)
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
            if (flameEffect != null)
            {
                flameEffect.Play();
            }
        }
        else
        {
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
}