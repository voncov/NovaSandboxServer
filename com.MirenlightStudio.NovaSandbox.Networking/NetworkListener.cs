using LiteNetLib;
using Serilog;
using System.Net;
using System.Net.Sockets;

namespace com.MirenlightStudio.NovaSandbox.Networking
{
    public class NetworkListener : INetEventListener
    {
        private NetworkConfiguration _networkConfiguration;
        public Action<ConnectionRequest> onConnectRequest;

        public void OnConnectionRequest(ConnectionRequest request)
        {
            Log.Debug($"Connection request from: {request.RemoteEndPoint}");
            onConnectRequest.Invoke(request);
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            Log.Debug($"Network error {socketError} from: {endPoint}");
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            NetworkPacketBase? packet = NetworkPacketParser.Parse(reader);
            Log.Debug($"New packet from {peer.Address}:{peer.Port}; Protocol={packet?.Protocol}; Type={packet?.PacketType}; Size={packet?.Size}");
            Log.Debug($"\tData: {BitConverter.ToString(reader.RawData)}");
            if (packet != null)
            {
                NetworkPacketHandlerRegistry.Handle(packet.PacketType, packet, peer);
            }
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
        }

        public void OnPeerConnected(NetPeer peer)
        {
            Log.Debug($"Connected: {peer.Address}");
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
        }

        public NetworkListener(NetworkConfiguration netCfg)
        {
            _networkConfiguration = netCfg;
        }
    }
}
