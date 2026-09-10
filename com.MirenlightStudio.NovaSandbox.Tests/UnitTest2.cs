using com.MirenlightStudio.NovaSandbox.Networking;
using com.MirenlightStudio.NovaSandbox.Networking.Packets;
using Serilog;

namespace com.MirenlightStudio.NovaSandbox.Tests;

public class UnitTest2
{
    [Fact]
    public async Task Login_ValidCredentials_ReturnsSuccess()
    {
        Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss}] [{Level}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                $"logs/log_{DateTime.UtcNow:MM-dd-yyyy}.log",
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();
        using Database database = new(new()
        {
            Host = "localhost",
            Port = 3306,
            Username = "root",
            Password = "",
            Database = "novasandbox_db"
        });
        Assert.True(database.Connect(), "Check mysql server is started.");

        NetworkConfiguration cfg = new()
        {
            Host = "127.0.0.1",
            Port = 9978
        };
        using Main.NovaServer server = new(database, cfg);
        server.StartPump();

        using TestClient client = new();
        client.Connect(cfg.Host, (int)cfg.Port);

        await client.WaitConnectedAsync().WaitAsync(TimeSpan.FromSeconds(5));
        AuthResponsePacket response = await client.LoginAsync("test", "1234test1234").WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Equal((byte)AuthResultStatusEnum.Success, response.Status);
        Assert.Equal("testificate", response.Nickname);
    }
}
