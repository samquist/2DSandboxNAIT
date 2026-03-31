using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Bomb : InteractableObject
{
    [Header("Effects")]
    [SerializeField] private ParticleSystem explosionParticleEffect;
    
    [Header("Audio")]
    [SerializeField] private AudioClip fuseSound;
    [SerializeField] private AudioClip boomSound;

    private List<Rigidbody2D> objsWithinRange;

    private Rigidbody2D rb;
    [SerializeField] private float effectAreaRadius;
    private AudioSource audioSource;

    public float forceValue = 20f;
    [SerializeField] private float holdTimer;
    private float thresholdTime = 0.15f, thresholdVelocity = 0.25f;

    private DragAndScale dragAndScale;
    private bool isExploding = false;

    [SerializeField] private float scrollStepSize = 0.01f;

    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody2D>();
        dragAndScale = GetComponentInParent<DragAndScale>();

        audioSource = GetComponent<AudioSource>();
        objsWithinRange = new List<Rigidbody2D>();
        isExploding = false;
    }

    private void OnEnable()
    {
        Awake();
    }

    public override void OnGrabBegin(Vector2 screenPos)
    {
        dragAndScale.OnGrabBegin(screenPos);

        isDragging = true;
        holdTimer = 0f;
    }

    public override void OnGrabUpdate(Vector2 screenPos)
    {
        if (!isDragging)
        {
            return;
        }

        dragAndScale.OnGrabUpdate(screenPos);

        holdTimer += Time.deltaTime;
    }

    public override void OnGrabEnd()
    {
        if (!isDragging)
        {
            return;
        }

        dragAndScale.OnGrabEnd();

        if (holdTimer < thresholdTime && rb.linearVelocity.magnitude < thresholdVelocity && !isExploding)
        {
            StartCoroutine(LightBomb());
        }
    }

    public override void OnPinchBegin()
    { 
        dragAndScale.OnPinchBegin(); 
    }

    public override void OnPinchUpdate(float deltaDistance, float deltaRotationDegrees) 
    { 
        dragAndScale.OnPinchUpdate(deltaDistance, deltaRotationDegrees); 
    }

    public override void OnPinchEnd()
    {
        dragAndScale.OnPinchEnd();
    }

    public override void OnScrollPerformed(InputAction.CallbackContext ctx)
    {
        Vector2 scrollDelta = ctx.ReadValue<Vector2>();
        float scrollY = scrollDelta.y;

        if (Mathf.Abs(scrollY) <= 0.1f)
            return;

        float direction = Mathf.Sign(scrollY);
        float scaleDelta = direction * scrollStepSize;

        Vector3 currentScale = dragAndScale.transform.localScale;
        Vector3 newScale = currentScale + new Vector3(scaleDelta, scaleDelta, scaleDelta);

        newScale.x = Mathf.Clamp(newScale.x, dragAndScale.minScale, dragAndScale.maxScale);
        newScale.y = newScale.x;
        newScale.z = newScale.x;

        dragAndScale.transform.localScale = newScale;

        Vector3 currentPosition = dragAndScale.transform.position;
        Vector3 newPosition = currentPosition + new Vector3(scaleDelta, scaleDelta, 0) / 2f;
        dragAndScale.transform.position = newPosition;

        if (isPinched)
        {
            OnPinchEnd();
        }
    }

    private IEnumerator LightBomb()
    {
        isExploding = true;
        PlaySound(fuseSound);
        yield return new WaitForSeconds(fuseSound.length);
        //yield return new WaitForSeconds(1);
        StartCoroutine(Explode());
    }

    private IEnumerator Explode()
    {
        //store explosion position
        Vector3 explosionPosition = transform.position;
    
        //detach and play particle effect at explosion position
        if (explosionParticleEffect != null)
        {
            //detach and play particle
            explosionParticleEffect.transform.SetParent(null);
            explosionParticleEffect.transform.position = explosionPosition;
            explosionParticleEffect.Play();
            
            //destroy particle
            Destroy(explosionParticleEffect.gameObject, explosionParticleEffect.main.duration);
        }

        var hits = Physics2D.OverlapCircleAll(transform.position, effectAreaRadius * transform.lossyScale.x);
        foreach (var hit in hits)//gets all rigidbodies within effectAreaRadius
        {
            if (!hit.isTrigger && hit.gameObject.activeInHierarchy)
            {
                objsWithinRange.Add(hit.attachedRigidbody);
            }
        }

        List<Rigidbody2D> usedObjs = new List<Rigidbody2D>();
        foreach(var obj in objsWithinRange)//apply the force to each unique rigidbody only once
        {
            if (obj != null && !usedObjs.Contains(obj) && obj.bodyType == RigidbodyType2D.Dynamic && obj != rb)
            {
                Vector2 closestPoint = obj.ClosestPoint(new Vector2(transform.position.x, transform.position.y));
                Vector2 forceVector = (closestPoint - new Vector2(transform.position.x, transform.position.y));
                float distance = forceVector.magnitude;
                forceVector = forceVector.normalized * forceValue * transform.lossyScale.magnitude * transform.lossyScale.magnitude / (distance + 0.01f);
                obj.AddForceAtPosition(forceVector, closestPoint, ForceMode2D.Impulse);
                usedObjs.Add(obj);
            }
        }

        DisableBomb();
        
        PlaySound(boomSound);
        yield return new WaitForSeconds(boomSound.length);
        //yield return new WaitForSeconds(1);

        ResetBomb();
    }

    private void DisableBomb()
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

        foreach (var obj in GetComponentsInChildren<MeshRenderer>())//disable all bomb meshes
        {
            obj.enabled = false;
        }
        GetComponent<Collider2D>().enabled = false;

        Transform parent = transform.parent;
        transform.parent = null;
        Destroy(parent.gameObject);
    }

    private void ResetBomb()
    {
        gameObject.SetActive(false);
        foreach (var obj in GetComponentsInChildren<MeshRenderer>())//re-enable all bomb meshes
        {
            obj.enabled = true;
        }
        audioSource.clip = null;
        GetComponent<Collider2D>().enabled = true;
    }

    public void PlaySound(AudioClip sound, bool shouldLoop = false)
    {
        audioSource.Stop();
        audioSource.clip = sound;
        audioSource.loop = shouldLoop;
        audioSource.Play();
    }
}