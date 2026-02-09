using UnityEngine;
using UnityEngine.UI;

public class GravityController : MonoBehaviour
{
    [SerializeField] private Slider gravitySlider;
    [SerializeField] private float baseGravity = -9.81f;

    private void Awake()
    {
        if (gravitySlider == null)
        {
            Debug.LogError("Gravity slider not assigned!");
            return;
        }

        gravitySlider.minValue = 0f;
        gravitySlider.maxValue = 2f;
        gravitySlider.value = 1f;

        gravitySlider.onValueChanged.AddListener(UpdateGravity);

        UpdateGravity(gravitySlider.value);
    }

    private void UpdateGravity(float gravityMultiplier)
    {
        float newGravityY = baseGravity * gravityMultiplier;
        Physics2D.gravity = new Vector2(0f, newGravityY);
    }

    private void OnDestroy()
    {
        if (gravitySlider != null)
        {
            gravitySlider.onValueChanged.RemoveListener(UpdateGravity);
        }
    }
}