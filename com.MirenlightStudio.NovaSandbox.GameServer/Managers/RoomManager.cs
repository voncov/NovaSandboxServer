using com.MirenlightStudio.NovaSandbox.GameServer.MySql;
using com.MirenlightStudio.NovaSandbox.GameServer.Rooms;
using com.MirenlightStudio.NovaSandbox.Networking;
using com.MirenlightStudio.NovaSandbox.Networking.Packets;
using LiteNetLib;

namespace com.MirenlightStudio.NovaSandbox.GameServer.Managers
{
    public class RoomManager : IDisposable
    {
        private Database? _db;
        private bool _canUseDatabase;
        private Room[] _rooms;

        public RoomManager(Database database)
        {
            _rooms = new Room[255];
            _canUseDatabase = false;
            _db = database;
            _db.onDatabaseConnected += OnDatabaseConnected;
            _db.Connect();
            NetworkPacketHandlerRegistry.Register(NetworkPacketEnum.GetRoomListPacket, HandleGetRoomListPacket);
        }

        private void HandleGetRoomListPacket(NetworkPacketBase packet, NetPeer peer)
        {
            if (!_canUseDatabase)
            {
                peer.Disconnect();
                return;
            }

            if (packet is not GetRoomListPacket grp)
            {
                peer.Disconnect();
                return;
            }

            List<string> serializedRooms = new(_rooms.Length);

            foreach (Room room in _rooms)
            {
                if (room == null) continue;
                bool hasOwner = room.Owner != null && room.Owner.inRoom == true;
                bool hasPassword = room.Password != null && !string.IsNullOrEmpty(room.Password);

                serializedRooms.Add();
            }
        }

        public void Dispose()
        {
            if (_db != null) _db.onDatabaseConnected -= OnDatabaseConnected;
            NetworkPacketHandlerRegistry.Unregister(NetworkPacketEnum.GetRoomListPacket);
            _db = null;
        }

        private void OnDatabaseConnected()
        {
            _canUseDatabase = true;
        }
    }
}
