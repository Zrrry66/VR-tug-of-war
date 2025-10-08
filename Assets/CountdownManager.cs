using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections;

public class CountdownManager : NetworkBehaviour
{
    [Header("UI References")]
    public Button startButton; // Assign in Inspector (visible only for server)

    [Header("Audio")]
    public AudioClip countdownClip; // Single audio file with "3,2,1 Go!"
    private AudioSource audioSource;

    public GameObject NetworkStudyObject;
    public NetworkStudyManagerTOW networkStudy;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (startButton != null)
        {
            // Only server can see/use the Start button
            startButton.gameObject.SetActive(IsServer);
            startButton.onClick.AddListener(OnStartButtonClicked);
        }

        audioSource.playOnAwake = false;
        networkStudy = NetworkStudyObject.GetComponent<NetworkStudyManagerTOW>();
    }

    public void OnStartButtonClicked()
    {
        if (IsServer)
        {
            StartCountdownServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartCountdownServerRpc(ServerRpcParams rpcParams = default)
    {
        // Server tells all clients to play the countdown
        StartCountdownClientRpc();

        // Server starts its own countdown coroutine too
        StartCoroutine(StartGameAfterCountdown());
    }

    [ClientRpc]
    private void StartCountdownClientRpc(ClientRpcParams rpcParams = default)
    {
        PlayCountdown();
    }

    private void PlayCountdown()
    {
        if (countdownClip != null)
        {
            audioSource.PlayOneShot(countdownClip);
        }
        else
        {
            Debug.LogWarning("Countdown clip not assigned in inspector!");
        }
    }

    private IEnumerator StartGameAfterCountdown()
    {
        if (countdownClip != null)
        {
            yield return new WaitForSeconds(countdownClip.length);
        }

        // Now start the game after the sound is done
        networkStudy.setstartGame();
    }
}
