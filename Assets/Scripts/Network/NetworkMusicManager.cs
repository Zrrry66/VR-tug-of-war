using System;
using Unity.Netcode;
using UnityEngine;
using VRInSync.Utils;

namespace VRInSync.Network
{
    public class NetworkMusicManager : NetworkBehaviour
    {
        public AudioSource audioSource;
        private double networkToDspOffset;

        private void Awake()
        {
            if (audioSource == null)
                audioSource = GetComponentInChildren<AudioSource>();
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
            var startUtc = nowUtc.AddSeconds(5);
            long ticks = startUtc.Ticks;

            StartMusicClientRpc(ticks);
            Debug.Log($"[Server] scheduled global start time: {startUtc:O}");
        }

        // This RPC runs on every instance (server and clients)
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

            double scheduledDsp = dspNow + delay;
            audioSource.PlayScheduled(scheduledDsp);

            Debug.Log($"[Everyone] playback scheduled at DSP time = {scheduledDsp:F3}");
        }

        //stop music
        [Rpc(SendTo.Everyone)]
        public void StopMusicClientRpc()
        {
            audioSource.Stop();
        }

        // pause
        [Rpc(SendTo.Everyone)]
        public void PauseMusicClientRpc()
        {
            audioSource.Pause();
        }

        //resume
        [Rpc(SendTo.Everyone)]
        public void ResumeMusicClientRpc()
        {
            audioSource.UnPause();
        }
    }
}
