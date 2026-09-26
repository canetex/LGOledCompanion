using LGOledCompanion.Core;

namespace LGOledCompanion.Core.Tests;

public sealed class WakeOnLanTests
{
    [Fact]
    public void Packet_is_six_ff_then_mac_sixteen_times()
    {
        var packet = WakeOnLan.BuildPacket("AA:BB:CC:DD:EE:FF");
        Assert.Equal(102, packet.Length);
        Assert.All(packet.Take(6), b => Assert.Equal(0xFF, b));
        var mac = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF };
        for (var copy = 0; copy < 16; copy++)
        {
            Assert.Equal(mac, packet.Skip(6 + copy * 6).Take(6));
        }
    }

    [Fact]
    public void Invalid_mac_returns_empty()
    {
        Assert.Empty(WakeOnLan.BuildPacket("not-a-mac"));
        Assert.Empty(WakeOnLan.BuildPacket(""));
    }
}
