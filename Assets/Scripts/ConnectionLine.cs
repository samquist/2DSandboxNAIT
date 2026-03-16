using UnityEngine;

public class ConnectionLine : MonoBehaviour
{
    [SerializeField] private Transform p1, p2;
    [SerializeField] private Transform body, arrow;

    public void Initialize(Transform p1, Transform p2)
    {
        this.p1 = p1;
        this.p2 = p2;

    }

    private void Update()
    {
        SetMidpointPosition();
        SetArrowHeadPosition();
        SetScale();
        SetRotation();
    }

    private void SetMidpointPosition()
    {
        transform.position = (p1.position + p2.position) / 2;
    }

    private void SetArrowHeadPosition()
    {
        arrow.position = p2.position;
    }

    private void SetRotation()
    {
        //transform.rotation = Quaternion.LookRotation(p2.position - p1.position);
        transform.eulerAngles =new Vector3(0, 0, GetAngle(new Vector2(p1.position.x, p1.position.y), new Vector2(p2.position.x, p2.position.y)));
    }

    private void SetScale()
    {
        body.transform.localScale = new Vector3((p1.position - p2.position).magnitude,  body.transform.localScale.y, body.transform.localScale.z);
    }

    private float GetAngle(Vector3 p1, Vector3 p2)
    {
        float angle = 0;
        angle = Mathf.Atan2(p2.y - p1.y, p2.x - p1.x) * 180.0f / Mathf.PI;
        return angle;
    }
}
