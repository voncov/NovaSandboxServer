using LiteNetLib.Utils;
#if UNITY_2021_3_OR_NEWER
#nullable enable
#endif
namespace com.MirenlightStudio.NovaSandbox.Networking.Packets
{
    public class RoomListResponsePacket : NetworkPacketBase
    {
        public int PlayerCountOnGameMode { get; set; }
        public int RoomCountOnGameMode { get; set; }
        public string[]? RoomSerializedListString { get; set; }

        public RoomListResponsePacket()
        {
            PacketType = NetworkPacketEnum.RoomListResponse;
        }

        static RoomListResponsePacket()
        {
            NetworkProtocolRegistry.Register(NetworkPacketEnum.RoomListResponse, () => new RoomListResponsePacket());
        }

        public override void Read(NetDataReader reader)
        {
            PlayerCountOnGameMode = reader.GetInt();
            RoomCountOnGameMode = reader.GetInt();
            RoomSerializedListString = reader.GetStringArray();
        }

        public override void Write(NetDataWriter writer)
        {
            writer.Put(PlayerCountOnGameMode);
            writer.Put(RoomCountOnGameMode);
            writer.PutArray(RoomSerializedListString);
        }
    }
}
