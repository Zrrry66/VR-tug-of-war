using UnityEngine;
using UnityEngine.XR;
using Unity.Netcode;   // Built-in Unity Netcode
using System.Collections;
using System.Collections.Generic;

public class SimpleHapticRepeater : NetworkBehaviour
{
    [Range(0f, 1f)] public float amplitude = 0.2f;
    public float duration = 3.5f;
    public float interval = 5f;
    public bool isHaptic = false;
    private Coroutine hapticCoroutine;

    public override void OnNetworkSpawn()
    {
        if (IsServer)  // only server runs the loop
        {
            hapticCoroutine = StartCoroutine(ServerLoop());
        }
    }

    private IEnumerator ServerLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            TriggerHapticsClientRpc(amplitude, duration);
        }
    }

    [ClientRpc] // runs on each client
    private void TriggerHapticsClientRpc(float amp, float dur)
    {
        // Locally vibrate this client’s controller
        if(!isHaptic){return;}
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