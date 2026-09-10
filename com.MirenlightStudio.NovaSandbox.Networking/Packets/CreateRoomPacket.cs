using LiteNetLib.Utils;
#if UNITY_2021_3_OR_NEWER
#nullable enable
#endif
namespace com.MirenlightStudio.NovaSandbox.Networking.Packets
{
    public class CreateRoomPacket : NetworkPacketBase
    {
        public string? HostToken { get; set; }
        public string? Uuid { get; set; }
        public byte GameMode { get; set; }
        public long Map { get; set; }
        public string? Name { get; set; }
        public string? Password { get; set; }
        public byte MaxPlayers { get; set; }
        public short MaxProps { get; set; }
        public bool PvPEnabled { get; set; }

        public CreateRoomPacket()
        {
            PacketType = NetworkPacketEnum.CreateRoom;
        }

        static CreateRoomPacket()
        {
            NetworkProtocolRegistry.Register(NetworkPacketEnum.CreateRoom, () => new CreateRoomPacket());
        }

        public override void Read(NetDataReader reader)
        {
            HostToken = reader.GetString();
            Uuid = reader.GetString(32);
            GameMode = reader.GetByte();
            Map = reader.GetLong();
            Name = reader.GetString(32);
            Password = reader.GetString(32);
            MaxPlayers = reader.GetByte();
            MaxProps = reader.GetShort();
            PvPEnabled = reader.GetBool();
        }

        public override void Write(NetDataWriter writer)
        {
            writer.Put(HostToken);
            writer.Put(Uuid);
            writer.Put(GameMode);
            writer.Put(Map);
            writer.Put(Name);
            writer.Put(Password);
            writer.Put(MaxPlayers);
            writer.Put(MaxProps);
            writer.Put(PvPEnabled);
        }
    }
}
