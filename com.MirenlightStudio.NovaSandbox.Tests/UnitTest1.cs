using com.MirenlightStudio.NovaSandbox.Networking.Packets;
using LiteNetLib.Utils;
namespace com.MirenlightStudio.NovaSandbox.Tests;

public class UnitTest1
{
    [Fact]
    public void CheckLoginResponseFields()
    {
        AuthResponsePacket packet = new()
        {
            Status = (byte)AuthResultStatusEnum.Success,
            Id = 1,
            Username = "test",
            Nickname = "testificate",
            Role = (byte)AccountRoleEnum.user,
            CreatedAt = 1700000000
        };

        byte[] data = packet.Serialize();

        NetDataReader reader = new(data);
        reader.GetByte();
        reader.GetByte();
        short size = reader.GetShort();
        Assert.Equal(size, reader.AvailableBytes);

        AuthResponsePacket parsed = new();
        parsed.Read(reader.GetRemainingBytes());

        Assert.Equal(packet.Status, parsed.Status);
        Assert.Equal(packet.Id, parsed.Id);
        Assert.Equal(packet.Username, parsed.Username);
        Assert.Equal(packet.Nickname, parsed.Nickname);
        Assert.Equal(packet.Role, parsed.Role);
        Assert.Equal(packet.CreatedAt, parsed.CreatedAt);
    }
}
