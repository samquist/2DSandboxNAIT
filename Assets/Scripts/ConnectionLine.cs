using UnityEngine;

public class ConnectionLine : MonoBehaviour
{
    [SerializeField] private Transform p1, p2;

    public void Initialize(Transform p1, Transform p2)
    {
        this.p1 = p1;
        this.p2 = p2;

    }

    private void Update()
    {
        SetMidpointPosition();
        SetScale();
        SetRotation();
    }

    private void SetMidpointPosition()
    {
        transform.position = (p1.position + p2.position) / 2;
    }

    private void SetRotation()
    {
        transform.rotation = Quaternion.LookRotation(p2.position - p1.position);
    }

    private void SetScale()
    {
        transform.localScale = new Vector3(transform.localScale.x,  transform.localScale.y, (p1.position - p2.position).magnitude);
    }

    public float GetAngle(Vector3 p1, Vector3 p2)
    {
        float angle = 0;
        angle = Mathf.Atan2(p2.y - p1.y, p2.x - p1.x) * 180.0f / Mathf.PI;
        return angle;
    }
}
