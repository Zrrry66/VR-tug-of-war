using UnityEngine;
using Unity.Netcode;
using System.IO; // for File IO
using System;   // for Guid

public class TimeCalculator : NetworkBehaviour
{
    private float startTime;
    private float endTime;
    private bool isTimerRunning = false;

    private string GroupId; // generate after StartTimerRPC
    public string timeToCompleteTheTask;

    public GameObject studyManager;
    public NetworkStudyManagerTOW networkStudyManager;
    public int TrailConfig = 0; // get from another script

    public GameObject Syncroniobject;
    public SyncroniCalculator syncCalc;
    public int TotalSyncroniCount; // handled externally
    
    public int Latency = 300; // handled externally


    public GameObject grab1;
    public GrabCollisionDetector cl1;
    public int pullPerformed1 = 10; // handled externally

    public GameObject userSpawnId1;
    public GetSetUserID userId1;

    public GameObject userSpawnId2;
    public GetSetUserID userId2;


    public GameObject grab2;
    public GrabCollisionDetector cl2;
    public int PullPerformed2 = 11; // handled externally

    private string filePath;
    int TrailId = 0;

    void Start()
    { 
        if(!IsServer)
        { return; }
        Debug.Log("Started ");
        networkStudyManager = studyManager.GetComponent<NetworkStudyManagerTOW>();
        syncCalc = Syncroniobject.GetComponent<SyncroniCalculator>();
        cl1 = grab1.GetComponent<GrabCollisionDetector>();
        cl2 = grab2.GetComponent<GrabCollisionDetector>();
        userId1 = userSpawnId1.GetComponent<GetSetUserID>();
        userId2 = userSpawnId2.GetComponent<GetSetUserID>();

    }


    public override void OnNetworkSpawn()
    {
        if (!IsServer)

        { return; }
        TrailId = 0;
        Debug.Log("Started Timer script");
        networkStudyManager = studyManager.GetComponent<NetworkStudyManagerTOW>();
        syncCalc = Syncroniobject.GetComponent<SyncroniCalculator>();
        cl1 = grab1.GetComponent<GrabCollisionDetector>();
        cl2 = grab2.GetComponent<GrabCollisionDetector>();
    }


    public void setGroupID(string gID)
    {
        Debug.Log("GroupID set "+gID);
        GroupId = gID;
    }

    private void Awake()
    {
        Debug.Log($"Awake to Create File");

        // Fixed path for Windows
        string folderPath = @"C:\Users\unity-developer\Desktop\tug of war";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        filePath = Path.Combine(folderPath, "TagofWarPilotStudy_Sanchi_Kirti_With Uid_2.csv");

        // If file does not exist, create header
        if (!File.Exists(filePath))
        {
            string header = "TrailID,Group_ID,StartTime,EndTime,TimeToComplete,TrailConfig,TotalSyncroniCount,Latency,PullPerformed1,PullPerformed2,UserId1,UserId2";
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
        //TrailId = Guid.NewGuid().ToString();

        Debug.Log($"Timer started. TrailId: {GroupId}, StartTime: {startTime}");
    }

    // Called from another script
    [Rpc(SendTo.Server)]
    public void StopTimerRPC()
    {
        if (!IsServer || !isTimerRunning) return;

        endTime = Time.time;
        isTimerRunning = false;

        float totalTime = endTime - startTime;
        timeToCompleteTheTask = totalTime.ToString("F2");

        Debug.Log($"Task completed in: {timeToCompleteTheTask} (TrailId: {GroupId})");

        TrailConfig = networkStudyManager.getConfig();
        Latency = networkStudyManager.getLatency();
        TotalSyncroniCount = syncCalc.getSyncroni();
        pullPerformed1 = cl1.getCollisionCount();
        PullPerformed2 = cl2.getCollisionCount();

        Debug.Log($"Calling save on file: {filePath}");
        // Save to CSV
        SaveTrailDataServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SaveTrailDataServerRpc()
    {

        string newLine = $"{TrailId},{GroupId},{startTime},{endTime},{timeToCompleteTheTask},{TrailConfig},{TotalSyncroniCount},{Latency},{pullPerformed1},{PullPerformed2},{userId1},{userId2}";
        File.AppendAllText(filePath, newLine + Environment.NewLine);
        TrailId++;
        Debug.Log($"Data saved to CSV: {filePath}");
    }
}
