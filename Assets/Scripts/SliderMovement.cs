using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SliderMovement : MonoBehaviour
{
    public Transform railStart;    // Start point of green cylinder
    public Transform railEnd;      // End point of green cylinder
    public Transform grabTarget;   // The object the user grabs (this box)

    private XRGrabInteractable grabInteractable;

    void Start()
    {
        grabInteractable = grabTarget.GetComponent<XRGrabInteractable>();
        grabInteractable.trackPosition = true;
        grabInteractable.trackRotation = false; // Disable rotation tracking
    }

    void Update()
    {
        if (grabInteractable.isSelected)
        {
            // Project current position onto rail direction
            Vector3 railDirection = (railEnd.position - railStart.position).normalized;
            Vector3 projected = Vector3.Project(grabTarget.position - railStart.position, railDirection);
            Vector3 clampedPosition = railStart.position + projected;

            // Clamp the projected movement between railStart and railEnd
            float distance = Vector3.Distance(railStart.position, railEnd.position);
            float clampedDistance = Mathf.Clamp(projected.magnitude, 0, distance);

            grabTarget.position = railStart.position + railDirection * clampedDistance;
        }
    }
}
