using UnityEngine;
using UnityEngine.UI;

public class ProgressBarControl : MonoBehaviour
{
    public Transform box;              // The moving box
    public Transform midpoint;         // The midpoint reference
    public Image sliderImage;          // The UI Image (must be Fill type)
    public float maxDistance = 5f;     // Distance from midpoint to max edge

    void Update()
    {
        if (box == null || midpoint == null || sliderImage == null) return;

        float boxZ = box.position.z;
        float midZ = midpoint.position.z;

        // Get offset from midpoint
        float offset = boxZ - midZ;

        // Normalize the value to 0 (back) - 1 (forward)
        float normalized = Mathf.InverseLerp(-maxDistance, maxDistance, offset);

        // Apply to Image fill amount
        sliderImage.fillAmount = normalized;
    }
}