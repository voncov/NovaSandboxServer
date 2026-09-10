namespace com.MirenlightStudio.NovaSandbox.GameServer.MySql
{
    public class ConnectionConfig
    {
        public string? Host { get; set; }
        public int? Port { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Database { get; set; }

        public override string ToString()
        {
            return $"Server={Host};Port={Port};User ID={Username};Password={Password};Database={Database}";
        }
    }
}
