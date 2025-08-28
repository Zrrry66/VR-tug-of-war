using UnityEngine;

public class WhistleSound : MonoBehaviour
{
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // Call PlayWhistle every 1 second, starting immediately
        InvokeRepeating("PlayWhistle", 0f, 30f);
    }

    void PlayWhistle()
    {
        audioSource.Play();
    }
}
