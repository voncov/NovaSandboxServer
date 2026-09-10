using MySqlConnector;
using Serilog;

namespace com.MirenlightStudio.NovaSandbox.GameServer.MySql
{
    public class Database : IDisposable
    {
        private ConnectionConfig? _connectionConfig;
        public Action? onDatabaseConnected;

        public bool Connect()
        {
            try
            {
                ExecuteScalar("SELECT 1");
                Log.Information("Database connection established");
                onDatabaseConnected!.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Failed to create connection: ");
            }
            return false;
        }

        public object? ExecuteScalar(string sql, params MySqlParameter[] parameters)
        {
            using MySqlConnection conn = new(_connectionConfig!.ToString());
            conn.Open();
            using MySqlCommand cmd = new(sql, conn);
            cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteScalar();
        }

        public int ExecuteNonQuery(string sql, params MySqlParameter[] parameters)
        {
            using MySqlConnection conn = new(_connectionConfig!.ToString());
            conn.Open();
            using MySqlCommand cmd = new(sql, conn);
            cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteNonQuery();
        }

        public List<T> ExecuteQuery<T>(string sql, Func<MySqlDataReader, T> mapper, params MySqlParameter[] parameters)
        {
            using MySqlConnection conn = new(_connectionConfig!.ToString());
            conn.Open();
            using MySqlCommand cmd = new(sql, conn);
            cmd.Parameters.AddRange(parameters);
            using MySqlDataReader reader = cmd.ExecuteReader();
            List<T> results = new();
            while (reader.Read())
            {
                results.Add(mapper(reader));
            }
            return results;
        }

        public void Dispose()
        {
            _connectionConfig = null;
        }

        public Database(ConnectionConfig connectionConfig)
        {
            _connectionConfig = connectionConfig;
        }
    }
}
