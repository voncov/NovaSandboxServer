using com.MirenlightStudio.NovaSandbox.GameServer.MySql;
using com.MirenlightStudio.NovaSandbox.Networking;
using Serilog;

namespace com.MirenlightStudio.NovaSandbox.GameServer
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            #region Register Logger
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss}] [{Level}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                $"logs/game/log_{DateTime.UtcNow:MM-dd-yyyy}.log",
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();
            #endregion
            Log.Information($"Starting NovaSandbox GameServer on {(Environment.Is64BitOperatingSystem ? "64 Bit" : "32 Bit")} {Environment.OSVersion} Operating system ({(Environment.Is64BitProcess ? "64 Bit" : "32 Bit")} process)");

            #region Register Database
            using Database database = new(new()
            {
                Host = "localhost",
                Port = 3306,
                Username = "root",
                Password = "",
                Database = "novasandbox_db"
            });
            //if (!database.Connect()) return;
            #endregion

            #region Startup Server
            NetworkConfiguration cfg = new()
            {
                Host = "localhost",
                Port = 7777
            };

            using NovaGameServer server = new(database, cfg);
            server.StartPump();
            #endregion

            Log.Information($"NovaGameServer running now on {cfg.Host}:{cfg.Port}");

            while (!Console.KeyAvailable)
            {
                Thread.Sleep(100);
            }

            Log.Warning("NovaGameServer is shutting down...");
        }
    }
}
