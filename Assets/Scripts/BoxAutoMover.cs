using System.Collections;
using UnityEngine;
using Unity.Netcode;
public class BoxAutoMover : NetworkBehaviour
{
    public float moveDistance = 1f; //how far to move along each time
    public float interval = 3f; // time interval between moves
    public float moveDuration = 1f; // duration over which the movement is smoothed

    private bool canMove = false;

    // Called when this NetworkBehaviour is spawned on the network.
    // Only run the movement coroutine on the server.
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            StartCoroutine(MoveRoutine());
        }
    }

    private IEnumerator MoveRoutine()
    {
        while (true)
        {
            // wait until GameManager enables movement
            yield return new WaitUntil(() => canMove);

            // Wait for the specified interval
            yield return new WaitForSeconds(interval);

            // double-check in case paused mid-wait
            if (!canMove)
                continue;

            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + Vector3.forward * moveDistance;

            float elapsed = 0f;

            while (elapsed < moveDuration)
            {
                // Calculate interpolation factor (0 to 1)
                float t = elapsed / moveDuration;

                // Smoothly interpolate position in world space
                transform.position = Vector3.Lerp(startPos, endPos, t);

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Ensure exact final position
            transform.position = endPos;
        }
    }

    // called by GameManager
    public void EnableMovement() => canMove = true;
    public void DisableMovement() => canMove = false;
}
