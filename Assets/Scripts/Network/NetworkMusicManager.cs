using System;
using Unity.Netcode;
using UnityEngine;
using VRInSync.Utils;

namespace VRInSync.Network
{
    public class NetworkMusicManager : NetworkBehaviour
    {
        public AudioSource goAudioSource;      // Plays "321Go"
        public AudioSource mainAudioSource;    // Plays "MainLoop"
        public AudioSource cheerAudioSource;  //for cheering
        private double networkToDspOffset;

        private void Awake()
        {
            if (goAudioSource == null || mainAudioSource == null)
            {
                var sources = GetComponentsInChildren<AudioSource>();
                if (sources.Length >= 2)
                {
                    goAudioSource = sources[0];
                    mainAudioSource = sources[1];
                    cheerAudioSource = sources[2];
                }
                else
                {
                    Debug.LogError("Not enough AudioSources assigned or found in children.");
                }
            }
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

        [Rpc(SendTo.Server)]
        public void RequestStartMusicServerRpc()
        {
            if (!IsServer) return;

            var nowUtc = NtpTime.GetNetworkTime();
            var startUtc = nowUtc.AddSeconds(1);  // buffer for sync
            long ticks = startUtc.Ticks;

            StartMusicClientRpc(ticks);
            Debug.Log($"[Server] Scheduled global start time: {startUtc:O}");
        }

        [Rpc(SendTo.Everyone)]
        private void StartMusicClientRpc(long startTimeTicks)
        {
           SyncOffset();

            var startUtc = new DateTime(startTimeTicks, DateTimeKind.Utc);
            double startSec = (startUtc - DateTime.UnixEpoch).TotalSeconds;
            double dspNow = AudioSettings.dspTime;
            double delay = (startSec - networkToDspOffset) - dspNow;
            if (delay < 0) delay = 0;

            // Scheduled DSP times
            double dsp321GoTime = dspNow + delay;
            double goClipLength = goAudioSource.clip.length;
            double dspMainLoopTime = dsp321GoTime + goClipLength;

            // Schedule all
            goAudioSource.PlayScheduled(dsp321GoTime);
            mainAudioSource.PlayScheduled(dspMainLoopTime);
            cheerAudioSource.PlayScheduled(dspMainLoopTime);

            Debug.Log($"[Everyone] '321Go' at DSP={dsp321GoTime:F3}, 'MainLoop' and 'Cheer' at DSP={dspMainLoopTime:F3}");
        }

        [Rpc(SendTo.Everyone)]
        public void StopMusicClientRpc()
        {
            goAudioSource.Stop();
            mainAudioSource.Stop();
            cheerAudioSource.Stop();
        }

        [Rpc(SendTo.Everyone)]
        public void PauseMusicClientRpc()
        {
            goAudioSource.Pause();
            mainAudioSource.Pause();
            cheerAudioSource.Pause();
        }

        [Rpc(SendTo.Everyone)]
        public void ResumeMusicClientRpc()
        {
            goAudioSource.UnPause();
            mainAudioSource.UnPause();
            cheerAudioSource.UnPause();
        }
    }
}
