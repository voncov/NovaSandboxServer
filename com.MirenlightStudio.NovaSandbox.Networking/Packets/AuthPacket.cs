using LiteNetLib.Utils;
#if UNITY_2021_3_OR_NEWER
#nullable enable
#endif
namespace com.MirenlightStudio.NovaSandbox.Networking.Packets
{
    public class AuthPacket : NetworkPacketBase
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }

        public AuthPacket()
        {
            PacketType = NetworkPacketEnum.Auth;
        }

        static AuthPacket()
        {
            NetworkProtocolRegistry.Register(NetworkPacketEnum.Auth, () => new AuthPacket());
        }

        public override void Read(NetDataReader reader)
        {
            Username = reader.GetString();
            Password = reader.GetString();
            Email = reader.GetString();
        }

        public override void Write(NetDataWriter writer)
        {
            writer.Put(Username);
            writer.Put(Password);
            writer.Put(Email);
        }
    }
}