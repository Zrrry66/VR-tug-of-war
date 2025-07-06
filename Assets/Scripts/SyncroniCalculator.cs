using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SyncroniCalculator : NetworkBehaviour
{
    private Queue<string> msgQueue = new Queue<string>();
    public GameObject targetObject;  // Assign via Inspector
    public float moveDistance = 2f;

    private void Start()
    {
        Debug.Log("SyncroniCalculator started.");
    }

    private void Update()
    {
        // Only the server processes the queue
        if (!IsServer) return;

        if (msgQueue.Count >= 2)
        {
            string pop1 = msgQueue.Dequeue();
            string pop2 = msgQueue.Dequeue();

            Debug.Log($"[Server] Dequeued messages: {pop1}, {pop2}");

            if ((pop1 == "ropeGrab1" && pop2 == "ropeGrab2") ||
                (pop1 == "ropeGrab2" && pop2 == "ropeGrab1"))
            {
                Debug.Log("[Server] Matching pair detected: triggering movement.");
                MoveObjectForward();
                msgQueue.Clear();
            }
            else
            {
                msgQueue.Clear();
                Debug.Log("[Server] Messages do not match the required pair.");
                msgQueue.Enqueue(pop1);
            }

            
            Debug.Log("[Server] Queue cleared after processing.");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SubmitGrabMessageServerRpc(string message)
    {
        msgQueue.Enqueue(message);
        Debug.Log($"[ServerRpc] Received and enqueued message from client: {message}");
    }

    private void MoveObjectForward()
    {
        if (targetObject != null)
        {
            Vector3 oldPos = targetObject.transform.position;
            Vector3 backwardZ = new Vector3(0, 0, -moveDistance);
            targetObject.transform.position += backwardZ;
            Vector3 newPos = targetObject.transform.position;


            Debug.Log($"[Server] Moved targetObject forward from {oldPos} to {newPos}");
        }
        else
        {
            Debug.LogWarning("[Server] targetObject is null — cannot move.");
        }
    }
}
