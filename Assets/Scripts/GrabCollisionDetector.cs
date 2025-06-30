using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
public class GrabCollisionDetector : NetworkBehaviour
{


    private float lastTriggerTime = -1f;
    public float cooldown = 0.5f;
    public int flag = 0;
    public int f2 = 0;
    public GameObject objectToMove;
    public float moveDistance = 2f;


    private Vector3 initialPosition;
    private Quaternion initialRotation;

    public float progressIncreaseAmount = 0.03f;

    [SerializeField] private Image progressBar;
    private NetworkVariable<float> progressValue = new NetworkVariable<float>(
       1f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);




    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        // rb.isKinematic = true; // Stops physics interactions (no jittering)
        rb.constraints = RigidbodyConstraints.FreezePositionX
               | RigidbodyConstraints.FreezePositionY
               | RigidbodyConstraints.FreezePositionZ
               | RigidbodyConstraints.FreezeRotationX
               | RigidbodyConstraints.FreezeRotationY
               | RigidbodyConstraints.FreezeRotationZ;
      

        flag = 0;
    }



    public void OnGrab()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.None;
    }


    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger enter called on " + gameObject.name);

        if (!IsOwner)
        {
            Debug.Log("Not the owner, ignoring trigger");
            return;
        }

        if (other.CompareTag("Point1") && flag == 0)
        {
            Debug.Log("Detect collision with point  Enter 1");
            flag = 1;
        }
        else if (other.CompareTag("Point2") && flag ==1)
        {
            Debug.Log("Detect collision with point Enter 2");
            flag = 0;
            // move a object to forwar to user
            if (objectToMove != null)
            {
           
                // Move object backward along global Z axis (reduce Z position)
                Vector3 backwardZ = new Vector3(0, 0, -moveDistance);
                objectToMove.transform.position += backwardZ;
            
                Debug.Log("Moved object forward.");
            }

            float newProgress = Mathf.Clamp01(progressValue.Value + progressIncreaseAmount);
            progressValue.Value = newProgress;
            progressBar.fillAmount = progressValue.Value;
        }
        else
        {
            Debug.Log("Entered trigger with: " + other.tag);
        }
    }

    private void OnProgressChanged(float oldValue, float newValue)
    {
        if (IsClient && progressBar != null)
        {
            progressBar.fillAmount = newValue;
        }
    }
}