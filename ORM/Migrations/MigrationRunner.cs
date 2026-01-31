using System.Reflection;
using Npgsql;

namespace ORM.Migrations;

public class MigrationRunner
{
    private readonly string _connectionString;
    private const string MigrationTableName = "__migration_history";

    public MigrationRunner(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void EnsureMigrationTable()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var sql = $@"
            CREATE TABLE IF NOT EXISTS {MigrationTableName} (
                version VARCHAR(255) PRIMARY KEY,
                description TEXT,
                applied_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            );";

        using var cmd = new NpgsqlCommand(sql, connection);
        cmd.ExecuteNonQuery();
    }

    public List<string> GetAppliedMigrations()
    {
        EnsureMigrationTable();
        var applied = new List<string>();

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var sql = $"SELECT version FROM {MigrationTableName} ORDER BY applied_at";
        using var cmd = new NpgsqlCommand(sql, connection);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            applied.Add(reader.GetString(0));
        }
        return applied;
    }

    public List<Migration> DiscoverMigrations(Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(Migration).IsAssignableFrom(t))
            .Select(t => (Migration)Activator.CreateInstance(t)!)
            .OrderBy(m => m.Version)
            .ToList();
    }

    public void MigrateUp(Assembly assembly)
    {
        EnsureMigrationTable();
        var applied = GetAppliedMigrations();
        var migrations = DiscoverMigrations(assembly);
        var pending = migrations.Where(m => !applied.Contains(m.Version)).ToList();

        if (pending.Count == 0)
        {
            Console.WriteLine("No pending migrations.");
            return;
        }

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        foreach (var migration in pending)
        {
            Console.WriteLine($"Applying migration {migration.Version}: {migration.Description}");

            using var transaction = connection.BeginTransaction();
            try
            {
                var upSql = migration.Up();
                using var cmd = new NpgsqlCommand(upSql, connection, transaction);
                cmd.ExecuteNonQuery();

                var insertSql = $"INSERT INTO {MigrationTableName} (version, description) VALUES (@version, @description)";
                using var insertCmd = new NpgsqlCommand(insertSql, connection, transaction);
                insertCmd.Parameters.AddWithValue("@version", migration.Version);
                insertCmd.Parameters.AddWithValue("@description", migration.Description);
                insertCmd.ExecuteNonQuery();

                transaction.Commit();
                Console.WriteLine($"Migration {migration.Version} applied successfully.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"Migration {migration.Version} failed: {ex.Message}");
                throw;
            }
        }
    }

    public void MigrateDown(Assembly assembly, string? targetVersion = null)
    {
        EnsureMigrationTable();
        var applied = GetAppliedMigrations();
        var migrations = DiscoverMigrations(assembly);

        var toRollback = migrations
            .Where(m => applied.Contains(m.Version))
            .Where(m => targetVersion == null || string.Compare(m.Version, targetVersion) > 0)
            .OrderByDescending(m => m.Version)
            .ToList();

        if (toRollback.Count == 0)
        {
            Console.WriteLine("No migrations to rollback.");
            return;
        }

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        foreach (var migration in toRollback)
        {
            Console.WriteLine($"Rolling back migration {migration.Version}: {migration.Description}");

            using var transaction = connection.BeginTransaction();
            try
            {
                var downSql = migration.Down();
                using var cmd = new NpgsqlCommand(downSql, connection, transaction);
                cmd.ExecuteNonQuery();

                var deleteSql = $"DELETE FROM {MigrationTableName} WHERE version = @version";
                using var deleteCmd = new NpgsqlCommand(deleteSql, connection, transaction);
                deleteCmd.Parameters.AddWithValue("@version", migration.Version);
                deleteCmd.ExecuteNonQuery();

                transaction.Commit();
                Console.WriteLine($"Migration {migration.Version} rolled back successfully.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"Rollback of {migration.Version} failed: {ex.Message}");
                throw;
            }
        }
    }

    public void ShowStatus(Assembly assembly)
    {
        EnsureMigrationTable();
        var applied = GetAppliedMigrations();
        var migrations = DiscoverMigrations(assembly);

        Console.WriteLine("Migration Status:");
        Console.WriteLine(new string('-', 60));

        foreach (var migration in migrations)
        {
            var status = applied.Contains(migration.Version) ? "[Applied]" : "[Pending]";
            Console.WriteLine($"{status} {migration.Version}: {migration.Description}");
        }

        if (migrations.Count == 0)
        {
            Console.WriteLine("No migrations found.");
        }
    }
}
