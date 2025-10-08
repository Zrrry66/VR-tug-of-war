using UnityEngine;
using System.Collections;
using Unity.Netcode; // <- add NGO namespace

[RequireComponent(typeof(AudioSource))]
public class WhistleSound : NetworkBehaviour
{
    private AudioSource audioSource;


    public bool isSoundActivated = false;
    public NetworkVariable<bool> isBroadcasting = new NetworkVariable<bool>(
      false,
      NetworkVariableReadPermission.Everyone,
      NetworkVariableWritePermission.Server
  );


    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        Debug.Log("Server Started");
        // Only the server starts the coroutine
        if (IsServer)
        {
            Debug.Log("Coroutine started");
            StartCoroutine(RepeatingMessage());
        }
    }


    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Debug.Log("Coroutine started");
            StartCoroutine(RepeatingMessage());
        }
    }

    private void Update()
    {
        if(!IsServer)
        {
            return;
        }

        isBroadcasting.Value = isSoundActivated;

    }
    IEnumerator RepeatingMessage()
    {
        while (true)
        {
            // tell everyone to play the sound
            PlayWhistleClientRpc();

            yield return new WaitForSeconds(1f);

            // log on all clients
            LogMessageClientRpc("This message repeats every 3 seconds!");

            // stop the audio everywhere
            StopWhistleClientRpc();

            yield return new WaitForSeconds(2f);
        }
    }

    // ---- Broadcast Methods ----

    [ClientRpc]
    void PlayWhistleClientRpc()
    {
        if (!isBroadcasting.Value) return;
        audioSource.PlayOneShot(audioSource.clip);
    }

    [ClientRpc]
    void StopWhistleClientRpc()
    {
        audioSource.Stop();
    }

    [ClientRpc]
    void LogMessageClientRpc(string message)
    {
        Debug.Log(message);
    }

    public void activeSound()
    {
        isSoundActivated = true;
    }
    public void deActiveSound()
    {
        isSoundActivated = false;
    }
}