using UnityEngine;
using System.Collections;
using Unity.Netcode; // <- add NGO namespace
using Unity.Netcode;
using UnityEngine;
using System.Diagnostics;
using TMPro;
using UnityEngine.UI;
using System.IO;
using UnityEngine.InputSystem;
using VRSYS.Core.Avatar;
using VRSYS.Core.Networking;
using Debug = UnityEngine.Debug;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;


public class NetworkStudyManagerTOW : NetworkBehaviour
{



    readonly int[,] LatinSquare = new int[,]
        {
            {1, 2, 8, 3, 7, 4, 6, 5},
            {2, 3, 1, 4, 8, 5, 7, 6},
            {3, 4, 2, 5, 1, 6, 8, 7},
            {4, 5, 3, 6, 2, 7, 1, 8},
            {5, 6, 4, 7, 3, 8, 2, 1},
            {6, 7, 5, 8, 4, 1, 3, 2},
            {7, 8, 6, 1, 5, 2, 4, 3},
            {8, 1, 7, 2, 6, 3, 5, 4}
        };

    public GameObject soundBroadcast;
    public GameObject hapticBroadcast;
    public GameObject hapticSoundBroadcast;
    public GameObject gameManager;

    public WhistleSound wistlesoundBroadCast;
    public SimpleHapticRepeater simpleHapticBroadacast;
    public WhistleHapticBroadcast whistlHapticBroadcast;
    public NumberHandler numberHandler;

    public TimeCalculator timeCalculator;
    public int x;
    public int y;
    public int latency=300;

    public int condition=0;
    public int feedbackCondition = 0;


    public Boolean isGameStarted;

    //{}



    void Start()
    {
        CheckAdminAlternative();
        wistlesoundBroadCast = soundBroadcast.GetComponent<WhistleSound>();
        simpleHapticBroadacast = hapticBroadcast.GetComponent<SimpleHapticRepeater>();
        whistlHapticBroadcast = hapticSoundBroadcast.GetComponent<WhistleHapticBroadcast>();
        numberHandler = gameObject.GetComponent<NumberHandler>();
        timeCalculator = gameManager.GetComponent<TimeCalculator>();
        numberHandler.ReadNumberFromFile();
        x = numberHandler.getX();
        y = numberHandler.getY();

        feedbackCondition = LatinSquare[x,y];
        condition = feedbackCondition;
        timeCalculator.setGroupID("GroupID_" + x);
        Debug.Log("The Current Condition is :" + feedbackCondition);
        if(condition>=1 && condition<=4) { latency = 0; }
        if (condition >= 5 && condition <= 8) { latency = 400; }
        isGameStarted = false;
    }

    private void Update()
    {
        //Condition without latency 
        if (condition==1 && isGameStarted)
        {
            wistlesoundBroadCast.deActiveSound();
            simpleHapticBroadacast.deActiveHaptic();
            whistlHapticBroadcast.DisablConfig();
        }

        else if (condition == 2 && isGameStarted)
        {
            wistlesoundBroadCast.deActiveSound();
            simpleHapticBroadacast.activeHaptic();
            whistlHapticBroadcast.DisablConfig();
        }

        else if (condition == 3 && isGameStarted)
        {
            wistlesoundBroadCast.activeSound();
            simpleHapticBroadacast.deActiveHaptic();
            whistlHapticBroadcast.DisablConfig();
        }

        else if (condition == 4 && isGameStarted)
        {
            wistlesoundBroadCast.deActiveSound();
            simpleHapticBroadacast.deActiveHaptic();
            whistlHapticBroadcast.EnableConfig();
        }

        //Condition with latency 
        if (condition == 5 && isGameStarted)
        {
            wistlesoundBroadCast.deActiveSound();
            simpleHapticBroadacast.deActiveHaptic();
            whistlHapticBroadcast.DisablConfig();
        }

        else if (condition == 6 && isGameStarted)
        {
            wistlesoundBroadCast.deActiveSound();
            simpleHapticBroadacast.activeHaptic();
            whistlHapticBroadcast.DisablConfig();
        }

        else if (condition == 7 && isGameStarted)
        {
            wistlesoundBroadCast.activeSound();
            simpleHapticBroadacast.deActiveHaptic();
            whistlHapticBroadcast.DisablConfig();
        }

        else if (condition == 8 && isGameStarted)
        {
            wistlesoundBroadCast.deActiveSound();
            simpleHapticBroadacast.deActiveHaptic();
            whistlHapticBroadcast.EnableConfig();
        }


        //Useless condition no need
        else if (condition == 9 && isGameStarted)
        {
            wistlesoundBroadCast.activeSound();
            simpleHapticBroadacast.activeHaptic();
            whistlHapticBroadcast.DisablConfig();
        }

       

    }

    public void setstartGame()
    {
        isGameStarted = true;
    }

    public int getConfig()
    {
        return condition;
    }

    public int getLatency()
    {
        return latency;
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void SetLatencyRpc(int latency)
    {
        Debug.Log("Latency function called");
        //Process.Start("taskkill", "/IM clumsy.exe /F");
        var clumsyPath = @"C:\Users\unity-developer\Desktop\clumsy\clumsy.exe";//@"\C:\Users\unity-developer\Desktop\clumsy\clumsy.exe";

        if (!System.IO.File.Exists(clumsyPath))
        {
            Debug.LogError("Clumsy.exe not found!");
            return;
        }

        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = clumsyPath,
            Arguments = $" --filter \"outbound\" --lag on --lag-inbound off --lag-outbound on --lag-time {latency}", // latency parameters
            UseShellExecute = true, // needs to be true for window occurence
            Verb = "runas",  // for admin rights
            WorkingDirectory = System.IO.Path.GetDirectoryName(clumsyPath),
            WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal
        };

        Debug.Log("Command: " + psi.Arguments);

        try
        {
            System.Diagnostics.Process.Start(psi);
            Debug.Log("Clumsy was started.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Starting clumsy failed: " + ex.Message);
        }
    }

    public void RunClumsy()
    {
        Debug.Log("Clumsy Button clicked");
        SetLatencyRpc(latency);
    }

    public void ReRunClumsy()
    {
        RemoveLatencyRpc();
        SetLatencyRpc(latency);
    }


    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void RemoveLatencyRpc()
    {
        Process.Start("taskkill", "/IM clumsy.exe /F");
        Debug.Log("Latency was removed");
    }



    [ContextMenu("Check admin rights (alternative)")]
    public void CheckAdminAlternative()
    {
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c net session",
                Verb = "runas",    // to get admin rights, clumsy needs
                CreateNoWindow = true,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process proc = Process.Start(psi);
            proc.WaitForExit();

            bool isAdmin = (proc.ExitCode == 0);
            UnityEngine.Debug.Log("Unity is running with admin rights: " + isAdmin);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.Log("No admin rights or error: " + ex.Message);
        }
    }

    private IEnumerator DelayedQuit()
    {
        yield return new WaitForSeconds(5f); // z.B. 5 Sekunden anzeigen

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

}