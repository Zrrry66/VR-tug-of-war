using UnityEngine;
using System.Collections;
using Unity.Netcode; // <- add NGO namespace
using UnityEngine.XR;
using System.Collections.Generic;

public class WhistleHapticBroadcast : NetworkBehaviour
{

    [Header("Haptic Settings")]
    [Range(0f, 1f)] public float amplitude = 0.2f; // strength of vibration
    public float duration = 0.5f;                  // duration of each vibration
    public float interval = 3.5f;                  // repeat interval in seconds


    private AudioSource audioSource;

    private NetworkVariable<bool> isHapticSound = new NetworkVariable<bool>(
       false,
       NetworkVariableReadPermission.Everyone,
       NetworkVariableWritePermission.Server
       );


    [Header("Inspector-controlled Haptics")]
    [Tooltip("Enable/disable haptics from Inspector at runtime (server only)")]
    [SerializeField] private bool hapticDefault = false;



    private Coroutine hapticCoroutine;
    private bool lastInspectorValue;



    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        Debug.Log("Server Started Haptic Sound ");
        
    }



    private void Update()
    {
        if (!IsServer) return; // only server can update synced value
        if (hapticDefault != lastInspectorValue)
        {
            isHapticSound.Value = hapticDefault;
            lastInspectorValue = hapticDefault;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            isHapticSound.Value = hapticDefault; // initial value from Inspector
            lastInspectorValue = hapticDefault;
            Debug.Log("Server Started Haptic Sound Coroutine started");
            hapticCoroutine = StartCoroutine(ServerLoop());
        }
    }


    private IEnumerator ServerLoop()
    {
        // optional: trigger first vibration immediately
        TriggerHapticsSoundClientRpc(amplitude, duration);

        while (true)
        {
            yield return new WaitForSeconds(interval);
            Debug.Log("Sound Haptic played");
            TriggerHapticsSoundClientRpc(amplitude, duration);
            yield return new WaitForSeconds(1.5f);
            StopWhistleClientRpc();
        }
    }


    [ClientRpc]
    private void TriggerHapticsSoundClientRpc(float amp, float dur)
    {
        if (!isHapticSound.Value) return; // only vibrate if enabled
        VibrateLocalDevice(amp, dur);
        audioSource.PlayOneShot(audioSource.clip);
    }

    [ClientRpc]
    void StopWhistleClientRpc()
    {
        audioSource.Stop();
    }



    private void VibrateLocalDevice(float amp, float dur)
    {
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);

        foreach (var device in devices)
        {
            if (device.isValid &&
                device.TryGetHapticCapabilities(out var caps) &&
                caps.supportsImpulse)
            {
                device.SendHapticImpulse(0, Mathf.Clamp01(amp), dur);
            }
        }
    }


}
