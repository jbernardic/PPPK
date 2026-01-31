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

    private bool IsNavigationProperty(PropertyInfo prop)
    {
        var propType = prop.PropertyType;
        if (propType.IsGenericType)
        {
            var genericDef = propType.GetGenericTypeDefinition();
            if (genericDef == typeof(LazyLoader<>) || genericDef == typeof(LazyCollection<>))
                return true;
        }
        return false;
    }

    public string GenerateCreateTableSql(Type modelType)
    {
        var tableAttr = modelType.GetCustomAttribute<TableAttribute>();
        var tableName = tableAttr?.Name ?? modelType.Name.ToLower();

        var sb = new StringBuilder();
        sb.AppendLine($"CREATE TABLE IF NOT EXISTS {tableName} (");

        var properties = modelType.GetProperties().Where(p => !IsNavigationProperty(p)).ToArray();
        var columnDefs = new List<string>();
        var foreignKeys = new List<string>();

        foreach (var prop in properties)
        {
            var columnAttr = prop.GetCustomAttribute<ColumnAttribute>();
            var columnName = columnAttr?.Name ?? prop.Name.ToLower();
            var pgType = TypeMapper.GetPostgreSqlType(prop.PropertyType);
            var isPrimaryKey = prop.GetCustomAttribute<PrimaryKeyAttribute>() != null;
            var fkAttr = prop.GetCustomAttribute<ForeignKeyAttribute>();

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

            if (fkAttr != null)
            {
                var refTableAttr = fkAttr.ReferencedType.GetCustomAttribute<TableAttribute>();
                var refTableName = refTableAttr?.Name ?? fkAttr.ReferencedType.Name.ToLower();
                var refPkProp = fkAttr.ReferencedType.GetProperties()
                    .FirstOrDefault(p => p.GetCustomAttribute<PrimaryKeyAttribute>() != null);
                if (refPkProp != null)
                {
                    var refPkColAttr = refPkProp.GetCustomAttribute<ColumnAttribute>();
                    var refPkColumn = refPkColAttr?.Name ?? refPkProp.Name.ToLower();
                    foreignKeys.Add($"    FOREIGN KEY ({columnName}) REFERENCES {refTableName}({refPkColumn})");
                }
            }
        }

        var allDefs = columnDefs.Concat(foreignKeys);
        sb.AppendLine(string.Join(",\n", allDefs));
        sb.Append(");");

        return sb.ToString();
    }

    public List<Type> DiscoverModels(Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.GetCustomAttribute<TableAttribute>() != null)
            .ToList();
    }

    public List<Type> SortByDependencies(List<Type> models)
    {
        var sorted = new List<Type>();
        var visited = new HashSet<Type>();

        void Visit(Type type)
        {
            if (visited.Contains(type)) return;
            visited.Add(type);

            var props = type.GetProperties();
            foreach (var prop in props)
            {
                var fkAttr = prop.GetCustomAttribute<ForeignKeyAttribute>();
                if (fkAttr != null && models.Contains(fkAttr.ReferencedType))
                {
                    Visit(fkAttr.ReferencedType);
                }
            }
            sorted.Add(type);
        }

        foreach (var model in models)
        {
            Visit(model);
        }

        return sorted;
    }

    public void ExecuteMigration(Assembly assembly)
    {
        var models = DiscoverModels(assembly);
        models = SortByDependencies(models);

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
