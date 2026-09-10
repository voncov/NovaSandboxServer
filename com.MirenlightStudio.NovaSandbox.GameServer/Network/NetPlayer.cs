using com.MirenlightStudio.NovaSandbox.Static;
using LiteNetLib;

namespace com.MirenlightStudio.NovaSandbox.GameServer.Network
{
    public class NetPlayer : NetObject
    {
        private string? _nickname;
        private Vector3 _position;
        private Quaternion _rotation;
        private readonly float _maxSpeed;
        private readonly float _maxHealth;
        public bool inRoom;

        public NetPlayer(NetPeer peer, string nickname, Guid instanceGuid, Vector3 spawnPosition, Quaternion rotation, float maxSpeed, float maxHealth)
            : base(peer, $"[NetObj] ({CRC32.ComputeString(instanceGuid.ToString("N")):X8}): {nickname}", instanceGuid)
        {
            _nickname = nickname;
            _position = spawnPosition;
            _rotation = rotation;
            _maxSpeed = maxSpeed;
            _maxHealth = maxHealth;
        }

        public override void ObjectLogic()
        {
            throw new NotImplementedException();
        }
    }
}
