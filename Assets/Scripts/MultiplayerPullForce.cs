using UnityEngine;
using Unity.Netcode;

public class MultiplayerPullForce : NetworkBehaviour
{
    public Transform handle;                 // The player-controlled object (like rope handle)
    public Rigidbody sharedObject;           // The shared object that force will be applied to

    public float forceMultiplier = 20f;      // How strong the pulling effect is
    public float moveThreshold = 0.01f;      // Minimal movement before force is triggered

    private Vector3 lastPosition;

    void Start()
    {
        if (IsOwner)
        {
            lastPosition = handle.position;
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        Vector3 delta = handle.position - lastPosition;
        float forwardDelta = Vector3.Dot(delta, Vector3.forward);
        if (delta.magnitude > moveThreshold)
        {
            Vector3 force = Vector3.forward * forwardDelta * forceMultiplier;

            // Send the force to be applied on the server
            ApplyForceServerRpc(force);
            Debug.Log("Added Froce");
        }

        lastPosition = handle.position;
    }

    [ServerRpc]
    void ApplyForceServerRpc(Vector3 force)
    {
        Debug.Log("Apply force called Added Froce");
        if (sharedObject != null)
        {
            sharedObject.AddForce(force, ForceMode.Force);
        }
    }
}