using com.MirenlightStudio.NovaSandbox.GameServer.MySql;
using com.MirenlightStudio.NovaSandbox.Networking;
using LiteNetLib;

namespace com.MirenlightStudio.NovaSandbox.GameServer
{
    public class NovaGameServer : IDisposable
    {
        private readonly NetManager _server;
        private Thread? _pumpThread;
        private volatile bool _running;

        public NovaGameServer(Database database, NetworkConfiguration netCfg)
        {
            if (!NetworkProtocolRegistry.EnsureLoaded())
            {
                throw new InvalidOperationException("Failed to load network packets");
            }
            _server = new NetManager(new NetworkListener(netCfg));
            _server.Start(
                NetUtils.ResolveAddress(netCfg.Host),
                NetUtils.ResolveAddress("::1"),
                (int)netCfg.Port!
                );
        }

        public void StartPump()
        {
            _running = true;
            _pumpThread = new Thread(() =>
            {
                while (_running)
                {
                    _server.PollEvents();
                    Thread.Sleep(15);
                }
            })
            { IsBackground = true };
            _pumpThread.Start();
        }

        public void Dispose()
        {
            _running = false;
            _pumpThread?.Join(500);
            _server.Stop(true);
        }
    }
}
