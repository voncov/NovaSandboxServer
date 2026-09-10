using com.MirenlightStudio.NovaSandbox.Networking;
using com.MirenlightStudio.NovaSandbox.Networking.Packets;
using LiteNetLib;
using System.Net;
using System.Net.Sockets;

namespace com.MirenlightStudio.NovaSandbox.Tests
{
    public class TestClient : INetEventListener, IDisposable
    {
        private readonly NetManager _client;
        private Thread? _pumpThread;
        private volatile bool _running;
        private NetPeer? _peer;

        private readonly TaskCompletionSource<bool> _connectedTcs = new();
        private readonly TaskCompletionSource<AuthResponsePacket> _loginTcs = new();

        public TestClient()
        {
            _client = new NetManager(this);
            _client.Start();
        }

        public Task<AuthResponsePacket> LoginAsync(string username, string password)
        {
            AuthPacket packet = new()
            {
                Username = username,
                Password = password
            };
            _peer!.Send(packet.Serialize(), DeliveryMethod.ReliableOrdered);
            return _loginTcs.Task;
        }

        public void Connect(string host, int port)
        {
            _client.Connect(host, port, "");
            _running = true;
            _pumpThread = new(() =>
            {
                while (_running)
                {
                    _client.PollEvents();
                    Thread.Sleep(15);
                }
            })
            { IsBackground = true };
            _pumpThread.Start();
        }

        public Task WaitConnectedAsync()
        {
            return _connectedTcs.Task;
        }

        public void OnPeerConnected(NetPeer peer)
        {
            _peer = peer;
            _connectedTcs.TrySetResult(true);
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            NetworkPacketBase? packet = NetworkPacketParser.Parse(reader);
            if (packet is AuthResponsePacket response)
            {
                _loginTcs.TrySetResult(response);
            }
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
        }

        public void Dispose()
        {
            _running = false;
            _pumpThread?.Join(500);
            _client.Stop();
        }
    }
}
