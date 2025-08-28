using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class NetworkWhistleBroadcaster : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private float interval = 5f;     // repeat interval in seconds
    [SerializeField] private float staggerDelay = 2f; // delay between clients

    [Header("Inspector Control")]
    [SerializeField] private bool broadcastingDefault = true; // toggle from Inspector

    private AudioSource audioSource;
    private Coroutine whistleLoop;
    private bool lastInspectorValue;

    // Networked toggle for broadcasting
    public NetworkVariable<bool> isBroadcasting = new NetworkVariable<bool>(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

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
            // Initialize NetworkVariable from Inspector
            isBroadcasting.Value = broadcastingDefault;
            lastInspectorValue = broadcastingDefault;

            whistleLoop = StartCoroutine(WhistleLoop());
        }
    }

    private void Update()
    {
        if (!IsServer) return;

        // Detect Inspector changes at runtime
        if (broadcastingDefault != lastInspectorValue)
        {
            isBroadcasting.Value = broadcastingDefault;
            lastInspectorValue = broadcastingDefault;
        }
    }

    private IEnumerator WhistleLoop()
    {
        while (true)
        {
            if (isBroadcasting.Value)
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

            // Send RPC to this client
            ClientRpcParams rpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { client.ClientId }
                }
            };

            PlayWhistleClientRpc(delay, rpcParams);

            // If this client is also the host, play locally
            if (client.ClientId == NetworkManager.Singleton.LocalClientId)
            {
                StartCoroutine(PlayWithDelay(delay));
            }
        }
    }

    [ClientRpc]
    private void PlayWhistleClientRpc(float delay, ClientRpcParams rpcParams = default)
    {
        if (!isBroadcasting.Value) return; // respect server toggle
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
