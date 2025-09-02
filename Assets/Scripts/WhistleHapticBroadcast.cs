using UnityEngine;
using System.Collections;
using Unity.Netcode; // <- add NGO namespace
using UnityEngine.XR;
using System.Collections.Generic;

public class WhistleHapticBroadcast : NetworkBehaviour
{

    [Header("Audio")]
    public AudioClip whistleClip;
    private AudioSource audioSource;

    [Header("Haptic Settings")]
    [Range(0f, 1f)] public float amplitude = 0.5f;
    public float duration = 0.1f;
    public float sensitivity = 1.5f; // Higher = fewer beats detected

    private float[] samples = new float[512];
    private float[] historyBuffer = new float[43];
    private int bufferIndex = 0;

    private NetworkVariable<bool> isHapticSound = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [SerializeField] private bool hapticDefault = false;
    private bool lastInspectorValue;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        if (!IsServer) return;

        // Sync inspector toggle with clients
        if (hapticDefault != lastInspectorValue)
        {
            isHapticSound.Value = hapticDefault;
            lastInspectorValue = hapticDefault;

            if (hapticDefault && whistleClip != null)
            {
                audioSource.clip = whistleClip;
                audioSource.loop = true;
                audioSource.Play();
            }
            else
            {
                audioSource.Stop();
            }
        }

        if (isHapticSound.Value && audioSource.isPlaying)
        {
            if (DetectBeat())
            {
                TriggerHapticsSoundClientRpc(amplitude, duration);
            }
        }
    }

    private bool DetectBeat()
    {
        audioSource.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);

        // Compute average energy in spectrum
        float instantEnergy = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            instantEnergy += samples[i] * samples[i];
        }

        // Compute average from history buffer
        float avgEnergy = 0f;
        foreach (float val in historyBuffer) avgEnergy += val;
        avgEnergy /= historyBuffer.Length;

        // Store current energy
        historyBuffer[bufferIndex] = instantEnergy;
        bufferIndex = (bufferIndex + 1) % historyBuffer.Length;

        // Beat detected if energy is much higher than average
        return instantEnergy > avgEnergy * sensitivity;
    }

    [ClientRpc]
    private void TriggerHapticsSoundClientRpc(float amp, float dur)
    {
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


}
