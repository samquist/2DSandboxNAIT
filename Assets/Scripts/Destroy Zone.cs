using UnityEngine;

public class DestroyZone : MonoBehaviour
{
    public void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(collision.gameObject);
    }
}
