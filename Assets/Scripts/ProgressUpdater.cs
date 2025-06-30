using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ProgressUpdater : NetworkBehaviour
{
    private NetworkVariable<float> progress = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public Image sliderImage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        progress.OnValueChanged += UpdateSliderFill;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsServer)
        {
            float newValue = progress.Value + (Time.deltaTime / 5.0f);
            if(newValue > 1.0f)
            {
                newValue -= Mathf.Floor(newValue);
            }
            progress.Value = newValue;
        }
    }

    private void UpdateSliderFill(float prevValue, float newValue)
    {
        sliderImage.fillAmount = newValue;
    }
}
