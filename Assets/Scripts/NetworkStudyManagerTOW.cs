using UnityEngine;
using System.Collections;
using Unity.Netcode; // <- add NGO namespace

public class NetworkStudyManagerTOW : NetworkBehaviour
{

    public GameObject soundBroadcast;
    public GameObject hapticBroadcast;


    public WhistleSound wistlesoundBroadCast;
    public SimpleHapticRepeater simpleHapticBroadacast;

    public int condition=0;


    void Start()
    {
        wistlesoundBroadCast = soundBroadcast.GetComponent<WhistleSound>();
        simpleHapticBroadacast = hapticBroadcast.GetComponent<SimpleHapticRepeater>();
    }

    private void Update()
    {
        if(condition==0)
        {
            wistlesoundBroadCast.deActiveSound();
            simpleHapticBroadacast.deActiveHaptic();
        }

        else if (condition == 1)
        {
            wistlesoundBroadCast.deActiveSound();
            simpleHapticBroadacast.activeHaptic();
        }

        else if (condition == 2)
        {
            wistlesoundBroadCast.activeSound();
            simpleHapticBroadacast.deActiveHaptic();
        }

        else if (condition == 3)
        {
            wistlesoundBroadCast.activeSound();
            simpleHapticBroadacast.activeHaptic();
        }

    }

}