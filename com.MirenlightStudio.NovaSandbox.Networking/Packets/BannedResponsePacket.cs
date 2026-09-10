using LiteNetLib.Utils;
#if UNITY_2021_3_OR_NEWER
#nullable enable
#endif
namespace com.MirenlightStudio.NovaSandbox.Networking.Packets
{
    public class BannedResponsePacket : NetworkPacketBase
    {
        public string? Reason { get; set; }
        public long BannedBy { get; set; }
        public long BannedAt { get; set; }
        public long ExpiresAt { get; set; }

        public BannedResponsePacket()
        {
            ExpiresAt = 0;
            PacketType = NetworkPacketEnum.BannedResponse;
        }

        static BannedResponsePacket()
        {
            NetworkProtocolRegistry.Register(NetworkPacketEnum.BannedResponse, () => new BannedResponsePacket());
        }

        public override void Read(NetDataReader reader)
        {
            Reason = reader.GetString();
            BannedBy = reader.GetLong();
            BannedAt = reader.GetLong();
            ExpiresAt = reader.GetLong();
        }

        public override void Write(NetDataWriter writer)
        {
            writer.Put(Reason);
            writer.Put(BannedBy);
            writer.Put(BannedAt);
            writer.Put(ExpiresAt);
        }
    }
}
