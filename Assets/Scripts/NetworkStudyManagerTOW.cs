using UnityEngine;
using System.Collections;
using Unity.Netcode; // <- add NGO namespace

public class NetworkStudyManagerTOW : NetworkBehaviour
{

    public GameObject soundBroadcast;
    public GameObject hapticBroadcast;
    public GameObject hapticSoundBroadcast;

    public WhistleSound wistlesoundBroadCast;
    public SimpleHapticRepeater simpleHapticBroadacast;
    public WhistleHapticBroadcast whistlHapticBroadcast;

    public int condition=0;

    //{}



    void Start()
    {
        wistlesoundBroadCast = soundBroadcast.GetComponent<WhistleSound>();
        simpleHapticBroadacast = hapticBroadcast.GetComponent<SimpleHapticRepeater>();
        whistlHapticBroadcast = hapticSoundBroadcast.GetComponent<WhistleHapticBroadcast>();
    }

    private void Update()
    {
        if(condition==0)
        {
            wistlesoundBroadCast.deActiveSound();
            simpleHapticBroadacast.deActiveHaptic();
            whistlHapticBroadcast.DisablConfig();
        }

        else if (condition == 1)
        {
            wistlesoundBroadCast.deActiveSound();
            simpleHapticBroadacast.activeHaptic();
            whistlHapticBroadcast.DisablConfig();
        }

        else if (condition == 2)
        {
            wistlesoundBroadCast.activeSound();
            simpleHapticBroadacast.deActiveHaptic();
            whistlHapticBroadcast.DisablConfig();
        }

        else if (condition == 3)
        {
            wistlesoundBroadCast.activeSound();
            simpleHapticBroadacast.activeHaptic();
            whistlHapticBroadcast.DisablConfig();
        }

        else if(condition == 4) {
            wistlesoundBroadCast.deActiveSound();
            simpleHapticBroadacast.deActiveHaptic();
            whistlHapticBroadcast.EnableConfig();
        }

    }


    public int getConfig()
    {
        return condition;
    }



}