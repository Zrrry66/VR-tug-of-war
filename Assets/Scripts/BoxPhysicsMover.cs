using System.Collections;
using UnityEngine;
using Unity.Netcode;


public class BoxPhysicsMover : NetworkBehaviour
{
    public float impulseForce = 5f;
    public float interval = 3f;

    private Rigidbody rb;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        // Optionally adjust drag for extra resistance
        // rb.drag = 1f;

        StartCoroutine(ImpulseRoutine());
    }

    private IEnumerator ImpulseRoutine()
    {
        while (true)
        {
            // Wait for the next impulse moment
            yield return new WaitForSeconds(interval);

            // Apply a forward push
            rb.AddForce(Vector3.forward * impulseForce, ForceMode.Impulse);
        }
    }
}
