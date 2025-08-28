using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class NetworkWhistleBroadcaster : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private float interval = 500f;     // how often to repeat (seconds)
    [SerializeField] private float staggerDelay = 20f; // delay between players

    private AudioSource audioSource;
    private Coroutine whistleLoop;

    private void Awake()
    {
        // Get or add AudioSource
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

            // Tell this client to play whistle
            ClientRpcParams rpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { client.ClientId }
                }
            };

            PlayWhistleClientRpc(delay, rpcParams);

            // If this client is also the host, play whistle locally
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

        if (audioSource.clip != null)
        {
            audioSource.Play();
            Debug.Log($"Whistle played after {delay}s delay on client {NetworkManager.Singleton.LocalClientId}");
        }
        else
        {
            Debug.LogWarning("No AudioClip assigned on AudioSource!");
        }
    }

    private void OnDisable()
    {
        if (IsServer && whistleLoop != null)
            StopCoroutine(whistleLoop);
    }
}
