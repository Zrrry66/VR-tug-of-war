using UnityEngine;

public class RedObjectSpawner : MonoBehaviour
{
    [Header("Prefab and Center Settings")]
    public GameObject redObjectPrefab;     // Prefab to spawn
    public Transform centerObject;         // Green object (center)

    [Header("Spawn Configuration")]
    public int numberOfObjects = 8;        // Number of red objects
    public float radius = 5f;              // Radius from the green object

    void Start()
    {
        SpawnObjectsAroundCenter();
    }

    void SpawnObjectsAroundCenter()
    {
        if (redObjectPrefab == null || centerObject == null)
        {
            Debug.LogError("Missing prefab or center object reference.");
            return;
        }

        for (int i = 0; i < numberOfObjects; i++)
        {
            float angle = i * Mathf.PI * 2 / numberOfObjects;
            Vector3 spawnPos = new Vector3(
                Mathf.Cos(angle) * radius,
                0,
                Mathf.Sin(angle) * radius
            );

            // Offset relative to center position
            Vector3 worldPos = centerObject.position + spawnPos;

            Instantiate(redObjectPrefab, worldPos, Quaternion.identity);
        }
    }
}
