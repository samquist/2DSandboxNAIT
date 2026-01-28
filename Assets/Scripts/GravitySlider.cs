using UnityEngine;
using UnityEngine.UI;

public class GravityController : MonoBehaviour
{
    public Slider gravitySlider;
    private float baseGravity = -9.81f;
    
    void Start()
    {
        Physics2D.gravity = new Vector2(0, baseGravity);
        gravitySlider.value = 0;
        gravitySlider.onValueChanged.AddListener(ChangeGravity);
        
        // Print initial gravity
        Debug.Log("Gravity: " + baseGravity.ToString("F2"));
    }
    
    void ChangeGravity(float sliderValue)
    {
        float newGravity = baseGravity + sliderValue;
        Physics2D.gravity = new Vector2(0, newGravity);
        
        // Print updated gravity
        Debug.Log("Gravity: " + newGravity.ToString("F2"));
    }
}