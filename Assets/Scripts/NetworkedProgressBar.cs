using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkedProgressBar : NetworkBehaviour
{
    public Transform box;              // The moving box (owned by a player)
    public Transform midpoint;         // The fixed midpoint
    public Image sliderImage;          // The UI Image (Fill Type)
    public float maxDistance = 10f;     // Max +/- distance on the Z axis

    private NetworkVariable<float> normalizedProgress = new NetworkVariable<float>(
        0.5f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    void Update()
    {
        if (IsOwner)
        {
            // Only the owner calculates and sends progress to server
            float offset = box.position.z - midpoint.position.z;
            float normalized = Mathf.InverseLerp(-maxDistance, maxDistance, offset*(-1));
            UpdateProgressServerRpc(normalized);
        }

        // All clients (including host) update the UI locally
        sliderImage.fillAmount = normalizedProgress.Value;
    }

    [ServerRpc]
    void UpdateProgressServerRpc(float progress)
    {
        // Clamp between 0 and 1 and sync with all clients
        normalizedProgress.Value = Mathf.Clamp01(progress);
    }
}
