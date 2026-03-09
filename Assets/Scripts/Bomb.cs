using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : InteractableObject
{
    [Header("Effects")]
    [SerializeField] private ParticleSystem explosionParticleEffect;
    
    [Header("Audio")]
    [SerializeField] private AudioClip lightSound;
    [SerializeField] private AudioClip fuseSound;
    [SerializeField] private AudioClip boomSound;

    private List<Rigidbody2D> objsWithinRange = new List<Rigidbody2D>();

    private Rigidbody2D rb;
    [SerializeField] private float effectAreaRadius;
    private AudioSource audioSource;

    public float forceValue = 20f;
    [SerializeField] private float holdTimer;

    [SerializeField] private DragAndScale dragAndScale;

    private Vector3 startPos;
    private float thresholdTime = 0.15f, thresholdVelocity = 0.25f;

    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody2D>();

        audioSource = GetComponent<AudioSource>();
    }

    public override void OnGrabBegin(Vector2 screenPos)
    {
        dragAndScale.OnGrabBegin(screenPos);

        startPos = transform.position;
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

        if (holdTimer < thresholdTime && rb.linearVelocity.magnitude < thresholdVelocity)
        {
            StartCoroutine(LightBomb());
        }
    }

    public override void OnPinchEnd()
    {
        return;
    }

    private IEnumerator LightBomb()
    {
        //audioSource.Stop();
        //audioSource.clip = fuseSound;
        //audioSource.loop = false;
        //audioSource.Play();
        //yield return new WaitForSeconds(fuseSound.length);
        yield return new WaitForSeconds(1);
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

        var hits = Physics2D.OverlapCircleAll(transform.position, effectAreaRadius);
        foreach (var hit in hits)//gets all rigidbodies within effectAreaRadius
        {
            if (!hit.isTrigger)
            {
                objsWithinRange.Add(hit.attachedRigidbody);
            }
        }

        List<Rigidbody2D> usedObjs = new List<Rigidbody2D>();
        foreach(var obj in objsWithinRange)//apply the force to each unique rigidbody only once
        {
            if (!usedObjs.Contains(obj) && obj.bodyType == RigidbodyType2D.Dynamic && obj != rb)
            {
                Vector2 closestPoint = obj.ClosestPoint(new Vector2(transform.position.x, transform.position.y));
                Vector2 forceVector = (closestPoint - new Vector2(transform.position.x, transform.position.y));
                float distance = forceVector.magnitude;
                forceVector = forceVector.normalized / (distance + 0.001f) * forceValue;
                obj.AddForceAtPosition(forceVector, closestPoint, ForceMode2D.Impulse);
                usedObjs.Add(obj);
            }
        }

        GetComponent<Collider2D>().enabled = false;
        foreach (var obj in GetComponentsInChildren<MeshRenderer>())
        {
            obj.enabled = false;
        }
        //audioSource.Stop();
        //audioSource.clip = boomSound;
        //audioSource.loop = false;
        //audioSource.Play();
        //yield return new WaitForSeconds(boomSound.length);
        yield return new WaitForSeconds(1);

        Destroy(transform.parent.gameObject);
    }
}
