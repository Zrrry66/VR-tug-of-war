using Unity.Netcode;
using UnityEngine;

public class MultiplayerObjectSpawner : NetworkBehaviour
{
    public GameObject[] objectPrefabs; // Assign multiple different prefabs in the Inspector
    public int objectsPerRow = 4;
    public float row1OffsetX = 2f;
    public float row2OffsetX = 4f;
    public float verticalSpacing = 2f;

    public override void OnNetworkSpawn()
    {
        //SpawnCrowd();
    }

    public void SpawnCrowd()
    {
        SpawnRow(-row2OffsetX, "Row 2 Left");
        SpawnRow(-row1OffsetX, "Row 1 Left");
        SpawnRow(row1OffsetX, "Row 1 Right");
        SpawnRow(row2OffsetX, "Row 2 Right");
    }

    void SpawnRow(float offsetX, string rowName)
    {
        if (!IsServer || !IsHost)
            return;

        for (int i = 0; i < objectsPerRow; i++)
        {
            Vector3 spawnPos = transform.position + new Vector3(offsetX, 0, (i - objectsPerRow / 2f) * verticalSpacing);
            GameObject prefabToSpawn = objectPrefabs[Random.Range(0, objectPrefabs.Length)];
            GameObject obj = null;
            if (rowName== "Row 1 Right")
            {  obj = Instantiate(prefabToSpawn, spawnPos, Quaternion.Euler(0, 270 , 0)); }
            else if (rowName == "Row 2 Right")
            { obj = Instantiate(prefabToSpawn, spawnPos, Quaternion.Euler(0, 270, 0)); }
            else
            {
                obj = Instantiate(prefabToSpawn, spawnPos, Quaternion.Euler(0, 90, 0));
            }
            obj.name = $"{rowName}_Obj_{i}";
            var instanceNetworkObject = obj.GetComponent<NetworkObject>();
            instanceNetworkObject.Spawn();
        }
    }
}
