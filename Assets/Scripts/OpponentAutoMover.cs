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

    [SerializeField] private Image progressBar;

    //store initial position
    private Vector3 initialPosition; 
    private bool canMove = false;

    private NetworkVariable<float> progressValue = new NetworkVariable<float>(
        1f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        // cache initial position on spawn
        initialPosition = transform.position;

        if (IsServer)
        {
            StartCoroutine(MoveRoutine());
        }

        progressValue.OnValueChanged += OnProgressChanged;

        if (IsClient && progressBar != null)
        {
            progressBar.fillAmount = progressValue.Value;
        }
    }

    private IEnumerator MoveRoutine()
    {
        while (true)
        {
            //wait until gamemanager enables movement
            yield return new WaitUntil(() => canMove);

            yield return new WaitForSeconds(interval);

            // double check in case paused mid-wait
            if (!canMove)
                continue;

            Vector3 startPos = transform.position;

            // Move along the GLOBAL -Z axis (backward)
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

   
    private void OnProgressChanged(float oldValue, float newValue)
    {
        if (IsClient && progressBar != null)
        {
            progressBar.fillAmount = newValue;
        }
    }

    // called by game manager
    public void EnableMovement() => canMove = true;
    public void DisableMovement() => canMove = false;

    //reset to initial position
    public void ResetPosition()
    {
        transform.position = initialPosition;
    }
}
