using UnityEngine;
using UnityEngine.XR;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

public class SimpleHapticRepeater : NetworkBehaviour
{
    [Header("Haptic Settings")]
    [Range(0f, 1f)] public float amplitude = 0.2f; // strength of vibration
    public float duration = 0.5f;                  // duration of each vibration
    public float interval = 3.5f;                  // repeat interval in seconds

    [Header("Inspector-controlled Haptics")]
    [Tooltip("Enable/disable haptics from Inspector at runtime (server only)")]
    [SerializeField] private bool hapticDefault = false;

    // synced across network
    private NetworkVariable<bool> isHaptic = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private Coroutine hapticCoroutine;
    private bool lastInspectorValue;

    private void Update()
    {
        if (!IsServer) return; // only server can update synced value

        // detect Inspector change and sync to clients
        if (hapticDefault != lastInspectorValue)
        {
            isHaptic.Value = hapticDefault;
            lastInspectorValue = hapticDefault;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            isHaptic.Value = hapticDefault; // initial value from Inspector
            lastInspectorValue = hapticDefault;

            hapticCoroutine = StartCoroutine(ServerLoop());
        }
    }

    private IEnumerator ServerLoop()
    {
        // optional: trigger first vibration immediately
        TriggerHapticsClientRpc(amplitude, duration);

        while (true)
        {
            yield return new WaitForSeconds(interval);
            TriggerHapticsClientRpc(amplitude, duration);
        }
    }

    [ClientRpc]
    private void TriggerHapticsClientRpc(float amp, float dur)
    {
        if (!isHaptic.Value) return; // only vibrate if enabled
        VibrateLocalDevice(amp, dur);
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

    private void OnDisable()
    {
        if (IsServer && hapticCoroutine != null)
        {
            StopCoroutine(hapticCoroutine);
        }
    }
}
