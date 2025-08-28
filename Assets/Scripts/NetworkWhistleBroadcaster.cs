using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class NetworkWhistleBroadcaster : NetworkBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip whistleClip;   // assign in Inspector
    [SerializeField] private float interval = 5f;     // repeat interval (seconds)
    [SerializeField] private float staggerDelay = 2f; // delay between clients

    private AudioSource audioSource;
    private Coroutine whistleLoop;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            whistleLoop = StartCoroutine(WhistleLoop());
        }
    }

    private IEnumerator WhistleLoop()
    {
        while (true)
        {
            BroadcastWhistle();
            yield return new WaitForSeconds(interval);
        }
    }

    private void BroadcastWhistle()
    {
        if (!IsServer) return;

        var clients = NetworkManager.Singleton.ConnectedClientsList;

        for (int i = 0; i < clients.Count; i++)
        {
            var client = clients[i];
            float delay = i * staggerDelay;

            // Send RPC to each client (including host)
            ClientRpcParams rpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { client.ClientId }
                }
            };

            PlayWhistleClientRpc(delay, rpcParams);

            // If this client is also the host (server + client), play locally as well
            if (client.ClientId == NetworkManager.Singleton.LocalClientId)
            {
                StartCoroutine(PlayWithDelay(delay));
            }
        }
    }

    [ClientRpc]
    private void PlayWhistleClientRpc(float delay, ClientRpcParams rpcParams = default)
    {
        StartCoroutine(PlayWithDelay(delay));
    }

    private IEnumerator PlayWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (whistleClip != null)
        {
            audioSource.PlayOneShot(whistleClip);
            Debug.Log($"Whistle played after {delay}s delay on client {NetworkManager.Singleton.LocalClientId}");
        }
        else
        {
            Debug.LogWarning("Whistle clip not assigned!");
        }
    }

    private void OnDisable()
    {
        if (IsServer && whistleLoop != null)
            StopCoroutine(whistleLoop);
    }
}
