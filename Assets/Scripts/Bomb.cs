using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Header("Detonation (Long Hold)")]
    [SerializeField] private float holdToRemoveDuration = 1f;

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
    public bool isDragging { get; private set; }
    private float holdTimer;
    private bool isHoldingForDetonation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        audioSource = GetComponent<AudioSource>();

        effectArea.isTrigger = true;
        effectArea.radius = effectAreaRadius;
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
