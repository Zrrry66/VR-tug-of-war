using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using Unity.Netcode;
using UnityEngine.SceneManagement;
using VRInSync.Network;

public class NetworkedProgressBar : NetworkBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform box;                // The moving box (owned by a player)

    [SerializeField]
    private Transform midpoint;           // The fixed midpoint

    [SerializeField]
    private Image sliderImage;            // The UI Image (fillAmount)

    [SerializeField]
    private TMP_Text messageText;         // The UI TextMeshPro for win/lose messages

    [SerializeField]
    private Button restartButton;      // Reference to the Restart Button
    [SerializeField] 
    private Button reloadButton;

    public NetworkMusicManager musicManager;

    [Header("Progress Settings")]
    [SerializeField]
    private float maxDistance = 10f;      // Max +/- distance on the Z axis

    private bool gameEnded = false;       // Prevent multiple triggers

    private NetworkVariable<float> normalizedProgress = new NetworkVariable<float>(
        0.5f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    // Called on each client when this NetworkObject is spawned
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            // Ensure all references have been assigned
            if (box == null) Debug.LogError("[NetworkedProgressBar] box is not assigned！");
            if (midpoint == null) Debug.LogError("[NetworkedProgressBar] midpoint is not assigned！");
            if (sliderImage == null) Debug.LogError("[NetworkedProgressBar] sliderImage is not assigned！");
            if (messageText == null) Debug.LogError("[NetworkedProgressBar] messageText is not assigned！");
            if (restartButton == null) Debug.LogError("[ProgressBarNetwork] restartButton is not assigned!");

            // Hide win/lose text at start
            messageText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Only owner calculates and sends progress
        if (IsOwner && !gameEnded)
        {
            float offset = box.position.z - midpoint.position.z;
            // InverseLerp from -maxDistance..+maxDistance, invert direction via * -1
            float normalized = Mathf.InverseLerp(-maxDistance, maxDistance, offset * -1);
            UpdateProgressServerRpc(normalized);
        }

        // All clients update their UI fill amount
        sliderImage.fillAmount = normalizedProgress.Value;

        // Owner checks for win/lose and shows message
        if (IsOwner && !gameEnded)
        {
            float fill = normalizedProgress.Value;
            if (fill <= 0f)
                ShowEndMessage("You Lost!");
            else if (fill >= 1f)
                ShowEndMessage("You Win!");
        }
    }

    [ServerRpc]
    private void UpdateProgressServerRpc(float progress)
    {
        // Clamp and replicate to clients
        normalizedProgress.Value = Mathf.Clamp01(progress);
    }

    // ServerRpc to reset box position and progress
    [ServerRpc(RequireOwnership = false)]
    public void ResetProgressServerRpc()
    {
        box.position = midpoint.position;
        normalizedProgress.Value = 0.5f;
    }

    [ClientRpc]
    public void ClearEndStateClientRpc()
    {
        // hide end UI and clear the flag
        gameEnded = false;
        messageText.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        reloadButton.gameObject.SetActive(false);
        Time.timeScale = 1f;
        //box.position = midpoint.position;
    }


    /// <summary>
    /// Public call to reset progress and UI state
    /// </summary>
    public void ResetProgress()
    {
        ResetProgressServerRpc();

        // Only owner resets local UI state
        //if (IsOwner)
        //{
            gameEnded = false;
            messageText.gameObject.SetActive(false);
            restartButton.gameObject.SetActive(false);
            reloadButton.gameObject.SetActive(false);
            //Time.timeScale = 1f;
        //}
    }

    /// <summary>
    /// Display win/lose message and pause the local game
    /// </summary>
    /// <param name="msg">The message to display</param>
    private void ShowEndMessage(string msg)
    {
        messageText.gameObject.SetActive(true); // Make text visible
        messageText.text = msg;                 // Set content
        restartButton.gameObject.SetActive(true); // Show restart button
        //reloadButton.gameObject.SetActive(true);
        Time.timeScale = 0f;                    // Pause game locally
        gameEnded = true;                       // Prevent further updates
        musicManager.StopMusicClientRpc();
    }
}
/*using Unity.Netcode;
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

    // New: Reset both box position and progress back to midpoint + default
    [ServerRpc(RequireOwnership = false)]
    public void ResetProgressServerRpc()
    {
        box.position = midpoint.position;
        normalizedProgress.Value = 0.5f;
    }

    // Public wrapper: reset progress (can be called from any client)
    public void ResetProgress()
    {
        ResetProgressServerRpc();
    }
}
*/