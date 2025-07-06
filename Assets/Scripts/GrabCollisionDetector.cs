using UnityEngine;
using Unity.Netcode;

public class GrabCollisionDetector : NetworkBehaviour
{
    private float lastTriggerTime = -1f;
    public float cooldown = 0.5f;

    public int flag = 0;
    public int f2 = 0;

    public GameObject objectToMove; // This should be a NetworkObject in the scene
    public float moveDistance = 2f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    public float progressIncreaseAmount = 0.03f;

    public GameObject msgQueue;


    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionX
                       | RigidbodyConstraints.FreezePositionY
                       | RigidbodyConstraints.FreezePositionZ
                       | RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationY
                       | RigidbodyConstraints.FreezeRotationZ;

        flag = 0;
    }

    public void OnGrab()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.None;
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
            Debug.Log("Detect collision with Point1");
            flag = 1;
        }
        else if (other.CompareTag("Point2") && flag == 1)
        {
            Debug.Log("Detect collision with Point2");
            flag = 0;

            if (objectToMove != null)
            {
                Debug.Log("Calling MoveObjectServerRpc");
                MoveObjectServerRpc();
            }
        }
        else
        {
            Debug.Log("Entered trigger with: " + other.tag);
        }
    }

    [ServerRpc]
    void MoveObjectServerRpc()
    {
        if (objectToMove != null)
        {
            Vector3 backwardZ = new Vector3(0, 0, -moveDistance);
            objectToMove.transform.position += backwardZ;

            Debug.Log("Moved object on the server. New position: " + objectToMove.transform.position);
            string objName = gameObject.name;
            msgQueue.GetComponent<SyncroniCalculator>().SubmitGrabMessageServerRpc(objName);
            Debug.Log("Pulled Object name: " + objName);
        }
    }
}
