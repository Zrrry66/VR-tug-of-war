using UnityEngine;
using Unity.Netcode;
using System.IO; // for File IO
using System;   // for Guid

public class TimeCalculator : NetworkBehaviour
{
    private float startTime;
    private float endTime;
    private bool isTimerRunning = false;

    public string TrailId; // generate after StartTimerRPC
    public string timeToCompleteTheTask;
    public int TrailConfig = 0; // get from another script
    public int TotalSyncroniCount = 7; // handled externally
    public int Latency = 300; // handled externally
    public int pullPerformed1 = 10; // handled externally
    public int PullPerformed2 = 11; // handled externally

    private string filePath;

    private void Awake()
    {
        // Fixed path for Windows
        string folderPath = @"C:\Users\TagofWarTrail";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        filePath = Path.Combine(folderPath, "TrailData.csv");

        // If file does not exist, create header
        if (!File.Exists(filePath))
        {
            string header = "TrailId,TimeToComplete,TrailConfig,TotalSyncroniCount,Latency,PullPerformed1,PullPerformed2";
            File.WriteAllText(filePath, header + Environment.NewLine);
        }
    }


    // Called after button click (you said you’ll handle button binding)
    [Rpc(SendTo.Server)]
    public void StartTimerRPC()
    {
        if (!IsServer) return;

        startTime = Time.time;
        isTimerRunning = true;

        // Generate unique TrailId
        TrailId = Guid.NewGuid().ToString();

        Debug.Log($"Timer started. TrailId: {TrailId}, StartTime: {startTime}");
    }

    // Called from another script
    [Rpc(SendTo.Server)]
    public void StopTimerRPC()
    {
        if (!IsServer || !isTimerRunning) return;

        endTime = Time.time;
        isTimerRunning = false;

        float totalTime = endTime - startTime;
        timeToCompleteTheTask = totalTime.ToString("F2") + " seconds";

        Debug.Log($"Task completed in: {timeToCompleteTheTask} (TrailId: {TrailId})");

        // Save to CSV
        SaveTrailDataServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SaveTrailDataServerRpc()
    {
        string newLine = $"{TrailId},{timeToCompleteTheTask},{TrailConfig},{TotalSyncroniCount},{Latency},{pullPerformed1},{PullPerformed2}";
        File.AppendAllText(filePath, newLine + Environment.NewLine);

        Debug.Log($"Data saved to CSV: {filePath}");
    }
}
