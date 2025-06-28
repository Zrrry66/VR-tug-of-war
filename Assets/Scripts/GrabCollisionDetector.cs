using UnityEngine;
using Unity.Netcode;

public class GrabCollisionDetector : NetworkBehaviour
{


    private float lastTriggerTime = -1f;
    public float cooldown = 0.5f;
    public int flag = 0;
    public GameObject objectToMove;
    public float moveDistance = 2f;

    void Start()
    {
        flag = 0;
    }


    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger enter called on " + gameObject.name);

        if (!IsOwner)
        {
            Debug.Log("Not the owner, ignoring trigger");
            return;
        }

        if (other.CompareTag("Point1") && flag == 0)
        {
            Debug.Log("Detect collision with point  Enter 1");
            flag = 1;
        }
        else if (other.CompareTag("Point2") && flag ==1)
        {
            Debug.Log("Detect collision with point Enter 2");
            flag = 0;
            // move a object to forwar to user
            if (objectToMove != null)
            {
           
                // Move object backward along global Z axis (reduce Z position)
                Vector3 backwardZ = new Vector3(0, 0, -moveDistance);
                objectToMove.transform.position += backwardZ;
            
                Debug.Log("Moved object forward.");
            }
        }
        else
        {
            Debug.Log("Entered trigger with: " + other.tag);
        }
    }


}