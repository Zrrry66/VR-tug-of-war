using UnityEngine;
using Unity.Netcode;
using System.Collections;
using UnityEngine.XR;

public class GrabCollisionDetector : NetworkBehaviour
{
    //private float lastTriggerTime = -1f;
    public float cooldown = 0.5f;

    public int flag = 0;
    public int f2 = 0;

    public GameObject objectToMove;
    public float moveDistance = 2f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    public float progressIncreaseAmount = 0.03f;

    public GameObject msgQueue;

    private GameObject firstPoint;               // reference to the first-touched sphere
    private Coroutine firstTimeoutCoroutine;     // coroutine for first-touch timeout

    // Haptic device
    private InputDevice rightHandDevice;
    private bool hapticInitialized = false;

    //beat detector
    public BeatDetector beatDetector;
    public float beatVibrationAmplitude = 0.2f;
    public float beatVibrationDuration = 0.1f;

    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionX
                       | RigidbodyConstraints.FreezePositionY
                       | RigidbodyConstraints.FreezePositionZ
                       | RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationY
                       | RigidbodyConstraints.FreezeRotationZ;

        flag = 0;

        if (IsOwner && IsClient && beatDetector != null)
            beatDetector.OnBeatDetected.AddListener(HandleBeatDetected);

    }

    void OnDisable()
   {
        if (IsOwner && IsClient && beatDetector != null)
            beatDetector.OnBeatDetected.RemoveListener(HandleBeatDetected);
    }

   // when detected whistle, call vibration
   public void HandleBeatDetected()
   {
        if (!IsOwner || !IsClient) return;  // only local owner vibrates
        StartCoroutine(Vibrate(beatVibrationAmplitude, beatVibrationDuration));
   }

public void OnGrab()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.None;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger enter called on " + gameObject.name);
        
        //if (!IsOwner)
        //{
        //    Debug.Log("Not the owner, ignoring trigger");
        //    return;
        //}
        
        if (other.CompareTag("Point1") && flag == 0)
        {
            Debug.Log("Detect collision with Point1");
            flag = 1;
            
            // Start first-touch feedback:
            firstPoint = other.gameObject;
            SetColor(firstPoint, Color.green);// Sphere1 turns green
            // Cancel any previous timeout
            if (firstTimeoutCoroutine != null)
                StopCoroutine(firstTimeoutCoroutine);

            // Start timeout: if no Point2 within 3s, reset Sphere1
            firstTimeoutCoroutine = StartCoroutine(ResetFirstAfterTimeout());
        }
        else if (other.CompareTag("Point2") && flag == 1)
        {
            Debug.Log("Detect collision with Point2");
            flag = 0;
            
            // Second-touch feedback:
            GameObject secondPoint = other.gameObject;
            SetColor(secondPoint, Color.green);// Sphere2 turns green

            // Cancel first-touch timeout
            if (firstTimeoutCoroutine != null)
                StopCoroutine(firstTimeoutCoroutine);

            // Trigger haptic vibration on controller
            StartCoroutine(Vibrate(0.4f, 0.2f)); // 40% amplitude for 0.2s

            if (objectToMove != null)
            {
                Debug.Log("Calling MoveObjectServerRpc");
                MoveObjectServerRpc();
            }
            
            // Reset colors after 1 second
            StartCoroutine(ResetBothAfterDelay(secondPoint));
        }
        else
        {
            Debug.Log("Entered trigger with: " + other.tag);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void MoveObjectServerRpc()
    {
        if (objectToMove != null)
        {
            Vector3 backwardZ = new Vector3(0, 0, -moveDistance);
          //  objectToMove.transform.position += backwardZ;

            Debug.Log("Moved object on the server. New position: " + objectToMove.transform.position);
            string objName = gameObject.name;
            msgQueue.GetComponent<SyncroniCalculator>().SubmitGrabMessageServerRpc(objName);
            Debug.Log("Pulled Object name: " + objName);
        }
    }
 
    private IEnumerator ResetFirstAfterTimeout()
    {
        yield return new WaitForSeconds(3f);
        if (firstPoint != null)
            SetColor(firstPoint, Color.white);// reset to white
        firstPoint = null;
        firstTimeoutCoroutine = null;
        flag = 0;
    }

    // Reset colors after 1 second
    private IEnumerator ResetBothAfterDelay(GameObject secondPoint)
    {
        yield return new WaitForSeconds(1f);
        if (firstPoint != null)
            SetColor(firstPoint, Color.white);
        if (secondPoint != null)
            SetColor(secondPoint, Color.white);
        firstPoint = null;
    }

    /// <summary>
    /// Helper to set the color of a sphere's Renderer.
    /// </summary>
    /// <param name="obj">Sphere GameObject</param>
    /// <param name="col">Target Color</param>
    private void SetColor(GameObject obj, Color col)
    {
        Renderer rend = obj.GetComponent<Renderer>();
        if (rend != null)
            rend.material.color = col;
    }
    /// <summary>
    /// Initialize the right-hand haptic device.
    /// </summary>
    private void InitializeHapticDevice()
    {
        // Populate a list with devices at the RightHand node
        var devices = new System.Collections.Generic.List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);
        if (devices.Count > 0)
        {
            rightHandDevice = devices[0];
            hapticInitialized = true;
        }
    }

    /// <summary>
    /// Coroutine to vibrate the controller.
    /// </summary>
    /// <param name="amplitude">Haptic strength [0.0,1.0]</param>
    /// <param name="duration">Duration in seconds</param>
    private IEnumerator Vibrate(float amplitude, float duration)
    {
        if (!IsOwner || !IsClient) yield break;  // local guard

        if (!hapticInitialized)
            InitializeHapticDevice();

        if (rightHandDevice.isValid)
        {
            HapticCapabilities capabilities;
            // Check if device supports impulse
            if (rightHandDevice.TryGetHapticCapabilities(out capabilities) && capabilities.supportsImpulse)
            {
                uint channel = 0;
                // Send the haptic impulse
                rightHandDevice.SendHapticImpulse(channel, amplitude, duration);
            }
        }

        // Wait for the vibration duration
        yield return new WaitForSeconds(duration);

        // Stop any ongoing haptics
        rightHandDevice.StopHaptics();
    }
}
