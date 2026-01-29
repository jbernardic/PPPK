using Npgsql;

namespace ORM.Core;

public abstract class DbContext : IDisposable
{
    private readonly string _connectionString;
    private NpgsqlConnection? _connection;

    protected DbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public NpgsqlConnection GetConnection()
    {
        if (_connection == null)
        {
            _connection = new NpgsqlConnection(_connectionString);
            _connection.Open();
        }
        return _connection;
    }

    protected DbSet<T> Set<T>() where T : class, new()
    {
        return new DbSet<T>(this);
    }

    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();
    }
}
