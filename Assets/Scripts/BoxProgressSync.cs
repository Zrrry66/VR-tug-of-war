using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class BoxProgressSync : NetworkBehaviour
{
    [Header("Set These in the Inspector")]
    public Transform boxTransform;      // The object that moves on Z
    public Image sliderImage;     // The UI progress bar
    private float previousZ;
    [Header("Z Axis Range")]
    public float minZ = 0f;             // Minimum Z value (0% progress)
    public float maxZ = 10f;            // Maximum Z value (100% progress)

    // Synced Z position (owner updates it, others read it)
    private NetworkVariable<float> progressValue = new NetworkVariable<float>(
        writePerm: NetworkVariableWritePermission.Owner
    );
    public float progressDecreaseAmount = 0.05f;
    void Start()
    {
        previousZ = transform.position.z;
        progressValue.Value = 0.5f;
        sliderImage.fillAmount = progressValue.Value;
    }

    void Update()
    {
        // Only the owner updates the synced Z value
       
        // All clients (including the owner) update the progress bar

        float currentZ = transform.position.z;

        if (currentZ > previousZ)
        {
            Debug.Log("Moving Forward (positive Z)");
            float newValue = progressValue.Value + (Time.deltaTime / 50.0f);
            UpdateProgressBar(newValue);
            progressValue.Value = newValue;
            if (progressValue.Value >= 1) { progressValue.Value = 1.0f; }
        }
        else if (currentZ < previousZ)
        {
            Debug.Log("Moving Backward (negative Z)");
            float newValue = progressValue.Value - (Time.deltaTime / 5.0f);
            UpdateProgressBar(newValue);
            progressValue.Value = newValue;
            if (progressValue.Value <= 0) { progressValue.Value = 0.0f; }
        }
        
        previousZ = currentZ;
        Debug.Log("Progress Value" + progressValue.Value);
        
  

    }

    void UpdateProgressBar(float zValue)
    {
        sliderImage.fillAmount = zValue;
    }
}
