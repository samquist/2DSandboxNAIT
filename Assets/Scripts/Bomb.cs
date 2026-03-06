using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : InteractableObject
{
    [Header("Detonation (Long Hold)")]
    [SerializeField] private float holdToDetonateDuration = 1f;

    [Header("Audio")]
    [SerializeField] private AudioClip lightSound;
    [SerializeField] private AudioClip fuseSound;
    [SerializeField] private AudioClip boomSound;

    private List<Rigidbody2D> objsWithinRange = new List<Rigidbody2D>();

    private Rigidbody2D rb;
    [SerializeField] private CircleCollider2D effectArea;
    [SerializeField] private float effectAreaRadius;
    private AudioSource audioSource;

    public float forceValue = 20f;
    public override bool isDragging { get; protected set; }
    private float holdTimer;
    private bool isHoldingForDetonation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        audioSource = GetComponent<AudioSource>();

        effectArea.isTrigger = true;
        effectArea.radius = effectAreaRadius;
    }

    public override void OnGrabBegin()
    {
        isDragging = true;
        isHoldingForDetonation = true;
        holdTimer = 0f;
    }
    public override void OnGrabUpdate(Vector2 screenPos)
    {
        if (!isDragging)
        {
            return;
        }

        if (isHoldingForDetonation)
        {
            holdTimer += Time.deltaTime;

            if (holdTimer >= holdToDetonateDuration)
            {
                LightBomb();
                isHoldingForDetonation = false;
            }
        }
    }
    public override void OnGrabEnd()
    {
        if (!isDragging)
        {
            return;
        }
    }
    public override void OnPinchEnd()
    {
        return;
    }

    private IEnumerator LightBomb()
    {
        yield return null;
    }

    private IEnumerator Explode()
    {
        List<Rigidbody2D> usedObjs = new List<Rigidbody2D>();
        foreach(var obj in objsWithinRange)
        {
            if (!usedObjs.Contains(obj))
            {
                Vector2 closestPoint = obj.ClosestPoint(new Vector2(transform.position.x, transform.position.y));
                Vector2 forceVector = (closestPoint - new Vector2(transform.position.x, transform.position.y));
                float distance = forceVector.magnitude;
                forceVector = forceVector.normalized / distance * ;
                obj.AddForceAtPosition(closestPoint - new Vector2(transform.position.x, transform.position.y), closestPoint, ForceMode2D.Impulse);
                usedObjs.Add(obj);
            }
        }

        audioSource.clip = boomSound;
        audioSource.loop = false;
        audioSource.Play();
        yield return new WaitForSeconds(boomSound.length);

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.attachedRigidbody != null)
        {
            objsWithinRange.Add(other.attachedRigidbody);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.attachedRigidbody != null)
        {
            objsWithinRange.Remove(other.attachedRigidbody);
        }
    }
}
