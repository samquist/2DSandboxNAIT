using UnityEngine;

public class DestroyZone : MonoBehaviour
{
    public void OnCollisionEnter2D(Collision2D collision)
    {
        ObjectResetter[] or = collision.rigidbody.GetComponentsInChildren<ObjectResetter>();

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
