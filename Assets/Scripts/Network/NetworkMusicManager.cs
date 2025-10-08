using System;
using Unity.Netcode;
using UnityEngine;
using VRInSync.Utils;

namespace VRInSync.Network
{
    public class NetworkMusicManager : NetworkBehaviour
    {
        public AudioSource audioSource;
        public AudioSource audioSource2;
        public AudioSource audioSource3;
        public AudioSource audioSource4; // whistle only

        private double networkToDspOffset;

        private void Awake()
        {
            if (audioSource == null)
                audioSource = GetComponentInChildren<AudioSource>();
            if (audioSource2 == null)
                Debug.LogWarning("audioSource2 is not assigned");
            if (audioSource3 == null)
                Debug.LogWarning("audioSource3 is not assigned");
        }

        private void Start()
        {
            if (IsClient && !IsServer)
                SyncOffset();
        }

        private void SyncOffset()
        {
            var netUtc = NtpTime.GetNetworkTime();
            double netSec = (netUtc - DateTime.UnixEpoch).TotalSeconds;
            networkToDspOffset = netSec - AudioSettings.dspTime;
            Debug.Log($"[Client] NTP→DSP offset = {networkToDspOffset:F3}s");
        }

        // This RPC runs on the server when a client requests playback
        [Rpc(SendTo.Server)]
        public void RequestStartMusicServerRpc()
        {
            if (!IsServer) return;

            // get a fresh NTP time, add buffer, send ticks to all clients
            var nowUtc = NtpTime.GetNetworkTime();
            var startUtc = nowUtc.AddSeconds(2);
            long ticks = startUtc.Ticks;

            StartMusicClientRpc(ticks);
            Debug.Log($"[Server] scheduled global start time: {startUtc:O}");
        }

        [Rpc(SendTo.Everyone)]
        private void StartMusicClientRpc(long startTimeTicks)
        {
            // resync offset for best accuracy
            SyncOffset();

            var startUtc = new DateTime(startTimeTicks, DateTimeKind.Utc);
            double startSec = (startUtc - DateTime.UnixEpoch).TotalSeconds;
            double dspNow = AudioSettings.dspTime;
            double delay = (startSec - networkToDspOffset) - dspNow;
            if (delay < 0) delay = 0;

            /*double scheduledDsp = dspNow + delay;
            audioSource.PlayScheduled(scheduledDsp);

            Debug.Log($"[Everyone] playback scheduled at DSP time = {scheduledDsp:F3}");
            */
            double dspStartTime = dspNow + delay;

            // Play Audio 1
            audioSource.loop = false;
            audioSource.PlayScheduled(dspStartTime);

            // Calculate playtime for audio 2 and 3
            double audio1Length = audioSource.clip.length;
            double dspTimeFor23 = dspStartTime + audio1Length;

            // Play audio 2 and 3 simultaneously
            audioSource2.loop = true;
            audioSource3.loop = true;
            audioSource4.loop = true;
            audioSource2.PlayScheduled(dspTimeFor23);
            audioSource3.PlayScheduled(dspTimeFor23);
            audioSource4.PlayScheduled(dspTimeFor23);

            Debug.Log($"[Everyone] Audio1 scheduled at DSP time = {dspStartTime:F3}");
            Debug.Log($"[Everyone] Audio2/3 scheduled at DSP time = {dspTimeFor23:F3}");

        }

        //stop music
        [Rpc(SendTo.Everyone)]
        public void StopMusicClientRpc()
        {
            audioSource.Stop();
            audioSource2.Stop();
            audioSource3.Stop();
            audioSource4.Stop();
        }

        // pause
        [Rpc(SendTo.Everyone)]
        public void PauseMusicClientRpc()
        {
            audioSource.Pause();
            audioSource2.Pause();
            audioSource3.Pause();
            audioSource4.Pause();
        }

        //resume
        [Rpc(SendTo.Everyone)]
        public void ResumeMusicClientRpc()
        {
            audioSource.UnPause();
            audioSource2.UnPause();
            audioSource3.UnPause();
            audioSource4.UnPause();
        }
    }
}
