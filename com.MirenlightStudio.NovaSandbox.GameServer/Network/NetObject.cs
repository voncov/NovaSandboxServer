using LiteNetLib;
using Medo;

namespace com.MirenlightStudio.NovaSandbox.GameServer.Network
{
    public abstract class NetObject : INetObject
    {
        private readonly NetPeer _peer;
        private readonly Guid _ownerGuid;
        private readonly string _name;
        private readonly Guid _instanceGuid;
        private readonly Guid _objectGuid;

        public NetObject(NetPeer peer, string name, Guid instanceGuid)
        {
            _peer = peer;
            _ownerGuid = Uuid7.Parse(_peer.Tag.ToString()!);
            _name = name;
            _instanceGuid = Uuid7.FromGuid(instanceGuid);
            _objectGuid = Uuid7.NewGuid();
        }

        public abstract void ObjectLogic();
    }
}
