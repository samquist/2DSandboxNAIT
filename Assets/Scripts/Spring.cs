using UnityEngine;

public class Spring : MonoBehaviour
{
    public float K;
    public float friction;
    public float LRest { get => topRB.transform.localScale.y * 1.0f; }
    public Transform topBlock, bottomBlock, springModel;
    public Rigidbody2D bottomRB, topRB;

    private void Update()
    {
        Vector3 forceVector = bottomBlock.position - topBlock.position;
        bottomRB.AddForce(-forceVector.normalized * CalculateRestorativeForce(forceVector.magnitude) * (1 - friction) * Time.deltaTime / 2);
        topRB.AddForceAtPosition(forceVector.normalized * CalculateRestorativeForce(forceVector.magnitude) * (1 - friction) * Time.deltaTime / 2, new Vector2(topBlock.position.x, topBlock.position.y));

        springModel.localScale = new Vector3(springModel.localScale.x, forceVector.magnitude / topBlock.localScale.y / LRest, springModel.localScale.z);
        springModel.localPosition = new Vector3(springModel.localPosition.x, -0.525f * springModel.localScale.y, springModel.localPosition.z);

        bottomBlock.localEulerAngles = Vector3.zero;
        //bottomBlock.localPosition = new Vector3(0, bottomBlock.localPosition.y, bottomBlock.localPosition.z);
    }

    public float CalculateRestorativeForce(float l)
    {
        return K * (l - LRest);
    }//end of CalculateRestorativeForce

    private void RotateObjectsFaceEachOtherY(Transform t1, Transform t2)
    {
        t1.eulerAngles = new Vector3(0, 0, GetAngle(new Vector2(t1.position.x, t1.position.y), new Vector2(t2.position.x, t2.position.y)) + 90);
        t2.eulerAngles = new Vector3(0, 0, GetAngle(new Vector2(t2.position.x, t2.position.y), new Vector2(t1.position.x, t1.position.y)) + 90);
    }

    private float GetAngle(Vector3 p1, Vector3 p2)
    {
        return Mathf.Atan2(p2.y - p1.y, p2.x - p1.x) * 180.0f / Mathf.PI;
    }
}
