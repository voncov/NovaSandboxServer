using com.MirenlightStudio.NovaSandbox.GameServer.Network;
using LiteNetLib;
using Medo;
using System.Buffers;
using System.Numerics;

namespace com.MirenlightStudio.NovaSandbox.GameServer.Rooms
{
    [Serializable]
    public class Room : IDisposable
    {
        private NetPlayer? _owner;
        private readonly Guid _guid;
        private readonly RoomGameModeEnum _gameMode;
        private readonly Vector3[] _perRoomSpawnPoints;
        private readonly byte _map;
        private readonly string? _name;
        private string? _password;
        private readonly byte _maxPlayers;
        private readonly short _maxProps;
        private bool _pvpEnabled;
        private bool _buildingEnabled;
        private bool _vehiclesEnabled;
        private bool _jumpingEnabled;
        private NetPlayer[]? _currentNetPlayers;
        private ArrayPool<NetPlayer>? _playedNetPlayers;

        public NetPlayer Owner
        {
            get
            {
                return _owner!;
            }
        }
        public Guid Guid
        {
            get
            {
                return _guid;
            }
        }
        public RoomGameModeEnum GameMode
        {
            get
            {
                return _gameMode;
            }
        }
        public byte Map
        {
            get
            {
                return _map;
            }
        }
        public string Name
        {
            get
            {
                return _name!;
            }
        }
        public string Password
        {
            get
            {
                return _password!;
            }
            set
            {
                _password = value;
            }
        }
        public byte CurrentPlayers
        {
            get
            {
                return Convert.ToByte(_currentNetPlayers!.Length);
            }
        }
        public byte MaxPlayers
        {
            get
            {
                return _maxPlayers;
            }
        }
        public short CurrentProps
        {
            get
            {
                // TODO: This value must be assigned.
                return default;
            }
        }
        public short MaxProps
        {
            get
            {
                return _maxProps;
            }
        }
        public bool PvPEnabled
        {
            get
            {
                return _pvpEnabled;
            }
            set
            {
                _pvpEnabled = value;
            }
        }
        public bool BuildingEnabled
        {
            get
            {
                return _buildingEnabled;
            }
            set
            {
                _buildingEnabled = value;
            }
        }
        public bool VehiclesEnabled
        {
            get
            {
                return _vehiclesEnabled;
            }
            set
            {
                _vehiclesEnabled = value;
            }
        }
        public bool JumpingEnabled
        {
            get
            {
                return _jumpingEnabled;
            }
            set
            {
                _jumpingEnabled = value;
            }
        }

        public Room(NetPeer creator, RoomGameModeEnum gameMode, Vector3[] perRoomSpawnPoints, byte map, string name, string password, byte maxPlayers)
        {
            _guid = Uuid7.NewGuid();
            _gameMode = gameMode;
            _perRoomSpawnPoints = perRoomSpawnPoints;
            _map = map;
            _name = name;
            _password = password;
            _maxPlayers = maxPlayers;

            _owner = new NetPlayer(creator, creator.);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
