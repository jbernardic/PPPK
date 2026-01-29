using System.Reflection;
using System.Text;
using Npgsql;
using ORM.Attributes;
using ORM.Core;

namespace ORM.Schema;

public class SchemaGenerator
{
    private readonly string _connectionString;

    public SchemaGenerator(string connectionString)
    {
        _connectionString = connectionString;
    }

    public string GenerateCreateTableSql(Type modelType)
    {
        var tableAttr = modelType.GetCustomAttribute<TableAttribute>();
        var tableName = tableAttr?.Name ?? modelType.Name.ToLower();

        var sb = new StringBuilder();
        sb.AppendLine($"CREATE TABLE IF NOT EXISTS {tableName} (");

        var properties = modelType.GetProperties();
        var columnDefs = new List<string>();

        foreach (var prop in properties)
        {
            var columnAttr = prop.GetCustomAttribute<ColumnAttribute>();
            var columnName = columnAttr?.Name ?? prop.Name.ToLower();
            var pgType = TypeMapper.GetPostgreSqlType(prop.PropertyType);
            var isPrimaryKey = prop.GetCustomAttribute<PrimaryKeyAttribute>() != null;

            var columnDef = $"    {columnName} {pgType}";
            if (isPrimaryKey)
            {
                if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(long))
                {
                    columnDef = $"    {columnName} SERIAL";
                }
                columnDef += " PRIMARY KEY";
            }
            columnDefs.Add(columnDef);
        }

        sb.AppendLine(string.Join(",\n", columnDefs));
        sb.Append(");");

        return sb.ToString();
    }

    public List<Type> DiscoverModels(Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.GetCustomAttribute<TableAttribute>() != null)
            .ToList();
    }

    public void ExecuteMigration(Assembly assembly)
    {
        var models = DiscoverModels(assembly);
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        foreach (var model in models)
        {
            var sql = GenerateCreateTableSql(model);
            Console.WriteLine($"Creating table for {model.Name}...");
            Console.WriteLine(sql);
            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.ExecuteNonQuery();
            Console.WriteLine("Table created successfully.\n");
        }

        if (models.Count == 0)
        {
            Console.WriteLine("No models found with [Table] attribute.");
        }
    }
}
