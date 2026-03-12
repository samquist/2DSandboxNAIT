using UnityEngine;

public class Spring : MonoBehaviour
{
    public float K;
    public float friction;
    public float LRest { get => transform.localScale.y; }
    public Transform topBlock, bottomBlock, springModel;
    public Rigidbody2D bottomRB, topRB;

    private void Update()
    {
        Vector3 forceVector = bottomBlock.position - topBlock.position;
        bottomRB.AddForce(-forceVector.normalized * CalculateRestorativeForce(forceVector.magnitude) * (1 - friction) * Time.deltaTime / 2);
        topRB.AddForce(forceVector.normalized * CalculateRestorativeForce(forceVector.magnitude) * (1 - friction) * Time.deltaTime / 2);

        springModel.localScale = new Vector3(springModel.localScale.x, forceVector.magnitude / topBlock.localScale.y, springModel.localScale.x);
        springModel.localPosition = new Vector3(springModel.localPosition.x, -0.55f * springModel.localScale.y, springModel.localPosition.z);

        //topBlock.localPosition = new Vector3(0, topBlock.localPosition.y, topBlock.localPosition.z);
        //topBlock.localEulerAngles = Vector3.zero;
        //bottomBlock.localPosition = new Vector3(0, bottomBlock.localPosition.y, bottomBlock.localPosition.z);
        //bottomBlock.localEulerAngles = Vector3.zero;
    }

    public float CalculateRestorativeForce(float l)
    {
        return K * (l - LRest);
    }//end of CalculateRestorativeForce
}
