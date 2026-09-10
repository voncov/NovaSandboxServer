using LiteNetLib.Utils;
#if UNITY_2021_3_OR_NEWER
#nullable enable
#endif
namespace com.MirenlightStudio.NovaSandbox.Networking.Packets
{
    public class GetRoomListPacket : NetworkPacketBase
    {
        public byte GameMode { get; set; }

        public GetRoomListPacket()
        {
            PacketType = NetworkPacketEnum.GetRoomListPacket;
        }

        static GetRoomListPacket()
        {
            NetworkProtocolRegistry.Register(NetworkPacketEnum.GetRoomListPacket, () => new GetRoomListPacket());
        }

        public override void Read(NetDataReader reader)
        {
            GameMode = reader.GetByte();
        }

        public override void Write(NetDataWriter writer)
        {
            writer.Put(GameMode);
        }
    }
}
