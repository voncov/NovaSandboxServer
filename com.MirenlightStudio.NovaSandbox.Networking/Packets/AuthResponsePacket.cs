using LiteNetLib.Utils;
#if UNITY_2021_3_OR_NEWER
#nullable enable
#endif
namespace com.MirenlightStudio.NovaSandbox.Networking.Packets
{
    public class AuthResponsePacket : NetworkPacketBase
    {
        public short Status { get; set; }
        public long Id { get; set; }
        public string? Username { get; set; }
        public string? Nickname { get; set; }
        public byte Role { get; set; }
        public long CreatedAt { get; set; }

        public AuthResponsePacket()
        {
            PacketType = NetworkPacketEnum.AuthResponse;
        }

        static AuthResponsePacket()
        {
            NetworkProtocolRegistry.Register(NetworkPacketEnum.AuthResponse, () => new AuthResponsePacket());
        }

        public override void Read(NetDataReader reader)
        {
            Status = reader.GetShort();
            Id = reader.GetLong();
            Username = reader.GetString(128);
            Nickname = reader.GetString(32);
            Role = reader.GetByte();
            CreatedAt = reader.GetLong();
        }

        public override void Write(NetDataWriter writer)
        {
            writer.Put(Status);
            writer.Put(Id);
            writer.Put(Username);
            writer.Put(Nickname);
            writer.Put(Role);
            writer.Put(CreatedAt);
        }
    }
}