using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ConstrainedGrabMovement : MonoBehaviour
{
    public Transform point1; // Top point on the rope
    public Transform point2; // Bottom point on the rope

    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        // Optional: Disable physics during grab
        GetComponent<Rigidbody>().isKinematic = true;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        // Optional: Re-enable physics after release
        GetComponent<Rigidbody>().isKinematic = false;
    }

    void Update()
    {
        if (grabInteractable.isSelected)
        {
            ConstrainToRope();
        }
    }

    void ConstrainToRope()
    {
        // Get grabber (e.g., hand/controller) position
        Transform interactor = grabInteractable.interactorsSelecting[0].transform;
        Vector3 handPos = interactor.position;

        // Project the hand position onto the rope line
        Vector3 p1 = point1.position;
        Vector3 p2 = point2.position;
        Vector3 ropeDir = (p2 - p1).normalized;
        float ropeLength = Vector3.Distance(p1, p2);

        Vector3 toHand = handPos - p1;
        float dot = Vector3.Dot(toHand, ropeDir);
        float clampedDistance = Mathf.Clamp(dot, 0f, ropeLength);

        // Final constrained position
        Vector3 constrainedPosition = p1 + ropeDir * clampedDistance;
        transform.position = constrainedPosition;
    }
}
