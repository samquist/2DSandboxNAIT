using UnityEngine;

public class GizmosDraw : MonoBehaviour
{
    [SerializeField] private Color color = Color.yellow;
    private void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawSphere(transform.position, 0.1f);
    }
}
