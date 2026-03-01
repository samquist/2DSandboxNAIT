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
        //GetComponent<LineRenderer>().SetPositions(new Vector3[] { p1.position, p2.position });
    }

    private void SetMidpointPosition()
    {
        transform.position = (p1.position + p2.position) / 2;
    }

    private void SetRotation()
    {
        //float xRot = GetAngle(new Vector2(p1.position.y, p1.position.z), new Vector2(p2.position.y, p2.position.z));
        //if (p1.position.x < p2.position.x)
        //    xRot += 180;
        //float yRot = GetAngle(new Vector2(p1.position.x, -p1.position.z), new Vector2(p2.position.x, -p2.position.z));
        //if (p1.position.y < p2.position.y)
        //    yRot += 180;
        //float zRot = GetAngle(new Vector2(p1.position.x, p1.position.y), new Vector2(p2.position.x, p2.position.y));
        //transform.eulerAngles = new Vector3(xRot, yRot, zRot);

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
