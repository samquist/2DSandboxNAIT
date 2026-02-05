using UnityEngine;
using UnityEngine.UI;

public class GravityController : MonoBehaviour
{
    [SerializeField] private Slider gravitySlider;
    [SerializeField] private float baseGravity = -9.81f;
    [SerializeField] private float maxMultiplier = 10f;

    private void Start()
    {
        if (gravitySlider == null)
        {
            Debug.LogError("Gravity slider not assigned!");
            return;
        }

        gravitySlider.minValue = -1f;
        gravitySlider.maxValue = 1f;
        gravitySlider.value = 0f;

        gravitySlider.onValueChanged.AddListener(UpdateGravity);

        UpdateGravity(0f);
    }

    private void UpdateGravity(float sliderValue)
    {
        float multiplier;

        if (sliderValue <= 0)
        {
            multiplier = 1f + (-sliderValue * (maxMultiplier - 1f));
        }
        else
        {
            multiplier = 1f - sliderValue;
        }

        float newGravityY = baseGravity * multiplier;
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