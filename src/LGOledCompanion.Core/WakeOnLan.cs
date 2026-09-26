// WakeOnLan.cs
// Top 5: BuildPacket O(1), ParseMac O(n), Send O(1)

using System.Net;
using System.Net.Sockets;

namespace LGOledCompanion.Core;

public static class WakeOnLan
{
    public static byte[] BuildPacket(string mac)
    {
        var bytes = ParseMac(mac);
        if (bytes.Length != 6)
        {
            return [];
        }

        var packet = new byte[102];
        Array.Fill(packet, (byte)0xFF, 0, 6);
        // O(1) 16 cópias fixas do MAC
        for (var copy = 0; copy < 16; copy++)
        {
            Buffer.BlockCopy(bytes, 0, packet, 6 + copy * 6, 6);
        }

        return packet;
    }

    public static bool Send(string mac)
    {
        var packet = BuildPacket(mac);
        if (packet.Length == 0)
        {
            return false;
        }

        try
        {
            using var udp = new UdpClient();
            udp.EnableBroadcast = true;
            udp.Send(packet, packet.Length, new IPEndPoint(IPAddress.Broadcast, 9));
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static byte[] ParseMac(string mac)
    {
        if (string.IsNullOrWhiteSpace(mac))
        {
            return [];
        }

        var hex = mac.Replace(":", "", StringComparison.Ordinal)
            .Replace("-", "", StringComparison.Ordinal)
            .Replace(".", "", StringComparison.Ordinal)
            .Trim();
        if (hex.Length != 12)
        {
            return [];
        }

        try
        {
            var bytes = new byte[6];
            // O(n) n = 6 octetos
            for (var i = 0; i < 6; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }

            return bytes;
        }
        catch
        {
            return [];
        }
    }
}
