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

    private NetworkVariable<float> progressValue = new NetworkVariable<float>(
        1f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
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

            // Server-only progress change
            float newProgress = Mathf.Clamp01(progressValue.Value - progressDecreaseAmount);
            progressValue.Value = newProgress;
        }
    }

    private void OnProgressChanged(float oldValue, float newValue)
    {
        if (IsClient && progressBar != null)
        {
            progressBar.fillAmount = newValue;
        }
    }
}
