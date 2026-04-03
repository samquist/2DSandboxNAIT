using UnityEngine;

public class DestroyZone : MonoBehaviour
{
    public void OnTriggerEnter2D(Collider2D other)
    {
        ObjectResetter[] or = other.attachedRigidbody.GetComponentsInChildren<ObjectResetter>();

        if (or.Length != 0)
        {
            for (int i = 0; i < or.Length; i++)
            {
                or[i].DeactivateObject();
            }
        }
        //else
        //{
        //    Destroy(collision.gameObject);
        //}
    }
}
