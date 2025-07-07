using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Linq;

public class PlayerTeleport : NetworkBehaviour
{
    // Holds all scene spawn points tagged "SpawnPoint"
    private Transform[] spawnPoints;

    // Tag of the UI Button in the scene
    private const string startButtonTag = "StartButton";
    private Button startButton;

    public override void OnNetworkSpawn()
    {
        // 1. Collect all SpawnPoint Transforms once
        GameObject[] spawns = GameObject.FindGameObjectsWithTag("SpawnPoint");
        spawnPoints = spawns
            .OrderBy(go => go.name) // Ensure SpawnPoint_0, SpawnPoint_1 order
            .Select(go => go.transform)
            .ToArray();

        // 2. Only the owning client sets up the UI listener
        if (!IsOwner) return;

        GameObject btnObj = GameObject.FindWithTag(startButtonTag);
        if (btnObj == null)
        {
            Debug.LogError("StartButton not found! Make sure it's tagged 'StartButton'.");
            return;
        }

        startButton = btnObj.GetComponent<Button>();
        if (startButton == null)
        {
            Debug.LogError("StartButton GameObject has no Button component!");
            return;
        }

        // Register click listener
        startButton.onClick.AddListener(OnStartClicked);
    }

    private void OnStartClicked()
    {
        // Client-side: request server to teleport all players
        TeleportAllServerRpc();
    }

    [ServerRpc]
    private void TeleportAllServerRpc(ServerRpcParams rpcParams = default)
    {
        // On server: loop through all connected clients
        foreach (var clientInfo in NetworkManager.Singleton.ConnectedClientsList)
        {
            // Get the player's spawned NetworkObject
            var playerObj = clientInfo.PlayerObject;
            if (playerObj == null) continue;

            // Determine index from ClientId (0,1,...)
            int idx = (int)clientInfo.ClientId;
            if (spawnPoints.Length == 0) continue;

            // If idx out of range, wrap around
            idx = idx % spawnPoints.Length;

            // Teleport that player's transform
            playerObj.transform.position = spawnPoints[idx].position;
        }
    }
}
