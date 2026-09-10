using com.MirenlightStudio.NovaSandbox.Static;
#nullable enable
namespace com.MirenlightStudio.NovaSandbox.GameServer.Rooms
{
    [NovaSerializerModel("room")]
    public class RoomInfo
    {
        [NovaSerializerId]
        public Guid RoomGuid { get; set; }

        [NovaSerializerFieldOrProperty("m")]
        public byte Map { get; set; }
        [NovaSerializerFieldOrProperty("n")]
        public string? Name { get; set; }
        [NovaSerializerFieldOrProperty("c")]
        public byte CountPlayers { get; set; }
        [NovaSerializerFieldOrProperty("x")]
        public byte MaxPlayers { get; set; }
        [NovaSerializerFieldOrProperty("l")]
        public bool Locked { get; set; }
        [NovaSerializerFieldOrProperty("o")]
        public bool Owned { get; set; }
    }
}
