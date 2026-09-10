using LiteNetLib.Utils;

namespace com.MirenlightStudio.NovaSandbox.Networking
{
    public abstract class NetworkPacketBase
    {
        public const byte ProtocolVersion = 0x010;
        public byte Protocol { get; set; }
        public NetworkPacketEnum PacketType { get; set; }
        public short Size { get; set; }

        public NetworkPacketBase()
        {
            Protocol = ProtocolVersion;
        }

        public abstract void Read(NetDataReader reader);
        public abstract void Write(NetDataWriter writer);

        public byte[] Serialize()
        {
            NetDataWriter payloadWriter = new();
            Write(payloadWriter);
            byte[] payload = payloadWriter.CopyData();

            NetDataWriter mainWriter = new();
            mainWriter.Put(Protocol);
            mainWriter.Put((byte)PacketType);
            mainWriter.Put((short)payload.Length);
            mainWriter.Put(payload);
            return mainWriter.CopyData();
        }

        public void Read(byte[] data)
        {
            Read(new NetDataReader(data));
        }
    }
}