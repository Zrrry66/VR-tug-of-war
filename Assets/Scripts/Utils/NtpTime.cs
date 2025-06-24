using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace VRInSync.Utils
{
    /// <summary>
    /// 基于 NTP（time.windows.com）获取 UTC 时间
    /// </summary>
    public static class NtpTime
    {
        private const string NtpServer = "time.windows.com";

        public static DateTime GetNetworkTime()
        {
            var ntpData = new byte[48];
            ntpData[0] = 0x1B; // LI=0, VN=3, Mode=3

            var addresses = Dns.GetHostEntry(NtpServer).AddressList;
            var endpoint = new IPEndPoint(addresses[0], 123);

            try
            {
                using (var sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    sock.Connect(endpoint);
                    sock.ReceiveTimeout = 3000;
                    sock.Send(ntpData);
                    sock.Receive(ntpData);
                }
            }
            catch (SocketException e)
            {
                Debug.LogError($"[NtpTime] NTP sync failed: {e.Message}");
                return DateTime.UtcNow;
            }

            const byte offset = 40;
            ulong intPart = SwapEndianness(BitConverter.ToUInt32(ntpData, offset));
            ulong fractPart = SwapEndianness(BitConverter.ToUInt32(ntpData, offset + 4));

            var milliseconds = (long)(intPart * 1000 + (fractPart * 1000) / 0x100000000L);
            var networkDateTime = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                      .AddMilliseconds(milliseconds);
            return networkDateTime;
        }

        private static uint SwapEndianness(ulong x)
        {
            return (uint)(((x & 0x000000FF) << 24) |
                          ((x & 0x0000FF00) << 8) |
                          ((x & 0x00FF0000) >> 8) |
                          ((x & 0xFF000000) >> 24));
        }
    }
}
