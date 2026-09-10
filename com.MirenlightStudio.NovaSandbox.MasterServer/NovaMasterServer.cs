using com.MirenlightStudio.NovaSandbox.MasterServer.MasterAccounts;
using com.MirenlightStudio.NovaSandbox.MasterServer.MySql;
using com.MirenlightStudio.NovaSandbox.Networking;
using LiteNetLib;

namespace com.MirenlightStudio.NovaSandbox.MasterServer
{
    public class NovaMasterServer : IDisposable
    {
        private AccountManager _accountManager;
        private readonly NetManager _server;
        private Thread? _pumpThread;
        private volatile bool _running;

        public NovaMasterServer(Database database, NetworkConfiguration netCfg)
        {
            if (!NetworkProtocolRegistry.EnsureLoaded())
            {
                throw new InvalidOperationException("Failed to load network packets");
            }
            _accountManager = new AccountManager(database);
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
