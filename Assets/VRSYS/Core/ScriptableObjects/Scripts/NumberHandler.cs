using UnityEngine;
using Unity.Netcode;
using System.IO;

public class NumberHandler : NetworkBehaviour
{
    // Network synchronized numbers
    private NetworkVariable<int> x = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> y = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Use absolute path (Windows)
    private string filePath = @"D:\VRinSyncRuye\numbers.txt";

    private void Start()
    {
        if (IsServer)
        {
            // If file does not exist, create it with default values
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "0 0");
            }
            ReadNumberFromFile();
        }
    }

    // Called when "Read" button is pressed
    [ServerRpc(RequireOwnership = false)]
    public void ReadNumberServerRpc()
    {
        ReadNumberFromFile();
    }

    // Called when "Write" button is pressed
    [ServerRpc(RequireOwnership = false)]
    public void WriteNumberServerRpc()
    {
        if (x.Value != 7)
        {
            x.Value++;
        }
        else
        {
            x.Value = 0;
            y.Value++;
        }

        File.WriteAllText(filePath, x.Value + " " + y.Value);
        Debug.Log("Server Wrote Numbers -> x: " + x.Value + ", y: " + y.Value);
    }

    public void ReadNumberFromFile()
    {
        if (File.Exists(filePath))
        {
            string content = File.ReadAllText(filePath);
            string[] parts = content.Split(' ');

            if (parts.Length >= 2)
            {
                int.TryParse(parts[0], out int readX);
                int.TryParse(parts[1], out int readY);
                x.Value = readX;
                y.Value = readY;
            }

            Debug.Log("Server Read Numbers -> x: " + x.Value + ", y: " + y.Value);
        }
        else
        {
            Debug.LogError("File not found: " + filePath);
        }
    }

    // Optional: display numbers locally on each client
    private void Update()
    {
        if (IsClient)
        {
            Debug.Log($"Synced Numbers -> x: {x.Value}, y: {y.Value}");
        }
    }

    //{}
    public int getX() {
        return y.Value;
    }

    public int getY() {
        return x.Value;
    }


}
