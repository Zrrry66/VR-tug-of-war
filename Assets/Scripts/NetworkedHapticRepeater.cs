using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR;

public class NetworkedHapticRepeater : NetworkBehaviour
{
    [Range(0f, 1f)] public float amplitude = 0.5f;
    public float duration = 0.2f;
    public float interval = 1f;

    private InputDevice rightHandDevice;
    private bool hapticInitialized = false;
    private Coroutine hapticCoroutine;

    void Start()
    {
        Debug.Log("Haptic Start");

            Debug.Log("Is Owner");
            hapticCoroutine = StartCoroutine(HapticLoop());
       
    }

    private void InitializeHapticDevice()
    {

        Debug.Log("Device Count");
        var devices = new System.Collections.Generic.List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);
        if (devices.Count > 0)
        {
            rightHandDevice = devices[0];
            hapticInitialized = true;
        }
    }

    private IEnumerator HapticLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            TriggerHapticClientRpc(amplitude, duration);
            Debug.Log("Haptic feedback___Call 1");
        }
    }

    [ClientRpc]
    void TriggerHapticClientRpc(float amp, float dur)
    {
        if (!hapticInitialized)
            InitializeHapticDevice();

        if (rightHandDevice.isValid)
        {
            Debug.Log("Haptic feedback___Call 2");

            HapticCapabilities capabilities;
            if (rightHandDevice.TryGetHapticCapabilities(out capabilities) && capabilities.supportsImpulse)
            {
                rightHandDevice.SendHapticImpulse(0, amp, dur);
            }
        }
    }

    private void OnDisable()
    {
        if (rightHandDevice.isValid)
            rightHandDevice.StopHaptics();

        if (hapticCoroutine != null)
            StopCoroutine(hapticCoroutine);
    }
}
