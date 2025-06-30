using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class OpponentAutoMover : NetworkBehaviour
{
    public float moveDistance = 1f;
    public float interval = 3f;
    public float moveDuration = 1f;
    public float progressDecreaseAmount = 0.05f;

 

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
            yield return new WaitForSeconds(interval);

            Vector3 startPos = transform.position;

            // ✅ Move along the GLOBAL -Z axis (backward)
            Vector3 endPos = startPos + Vector3.forward * moveDistance;

            float elapsed = 0f;
            while (elapsed < moveDuration)
            {
                float t = elapsed / moveDuration;
                transform.position = Vector3.Lerp(startPos, endPos, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = endPos;

        }
    }

   
}
