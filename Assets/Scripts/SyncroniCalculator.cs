using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
//{ }
public class SyncroniCalculator : NetworkBehaviour
{
    private Queue<string> msgQueue = new Queue<string>();
    public GameObject targetObject;  // Assign via Inspector
    public float moveDistance = 1.5f;

    public float threshold = 1.5f;
    public GameObject grabP1;
    public GameObject grabP2;

    private GrabCollisionDetector grabCollied1;
    private GrabCollisionDetector grabCollied2;


    private int SyncroniCount = 0;

    private void Start()
    {
        SyncroniCount = 0;
        Debug.Log("SyncroniCalculator started.");
        grabCollied1 = grabP1.GetComponent<GrabCollisionDetector>();
        grabCollied2 = grabP2.GetComponent<GrabCollisionDetector>();
        threshold = 5.5f;
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

                float eve1 = grabCollied1.GetColliedTime();
                float eve2 = grabCollied2.GetColliedTime();
                float eventDiff = Mathf.Abs(eve1 - eve2);
                Debug.Log("Time difference " + eventDiff);
                Debug.Log("[Server] Matching pair detected: triggering movement.");
                //MoveObjectForward();
                msgQueue.Clear();
               // SyncroniCount++;
                Debug.Log("Syncroni count "+SyncroniCount);
                if(eventDiff<=threshold)
                {
                    MoveObjectForward();
                    SyncroniCount++;
                    Debug.Log("Syncroni count " + SyncroniCount + "With time diff" + eventDiff);
                }
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

    public int getSyncroni() {
        return SyncroniCount;
    }

}
