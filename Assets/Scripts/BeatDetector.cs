using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;
using VRSYS.Core.Interaction.Samples;

/* 
 * 
 * The AudioSource connected to this script should contain only rythmic information (eg. isolated kick) 
 * 
*/

public class BeatDetector : NetworkBehaviour
{
    public UnityEvent OnBeatDetected;
    public AudioSource MyAudioSource;
    public AudioClip MyAudioClip;

    private float[] _spectrum = new float[512];
    [HideInInspector] public float AverageAmplitude;

    void Start()
    {
        //MyAudioSource = GetComponent<AudioSource>();
        //MyAudioClip = GetComponent<AudioClip>();
        if (MyAudioSource == null)
            MyAudioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        Listen();
    }

    public void Listen()
    {
        if (AverageAmplitude > 0)
        {
            OnBeatDetected?.Invoke();
        } 
    }
    
    void OnAudioFilterRead(float[] data, int channel)
    {
        int sum = 0;
        for (int i = 0; i < data.Length; i++)
        {
            if (data[i] > 0.01f)
                sum = 1;
        }
        AverageAmplitude = sum;
    }

}
