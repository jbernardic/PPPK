using System.Reflection;
using Npgsql;
using ORM.Attributes;

namespace ORM.Core;

public class LazyLoader<T> where T : class, new()
{
    private readonly DbContext _context;
    private readonly object _foreignKeyValue;
    private T? _value;
    private bool _loaded;

    public LazyLoader(DbContext context, object foreignKeyValue)
    {
        _context = context;
        _foreignKeyValue = foreignKeyValue;
        _loaded = false;
    }

    public T? Value
    {
        get
        {
            if (!_loaded)
            {
                _value = Load();
                _loaded = true;
            }
            return _value;
        }
    }

    private T? Load()
    {
        var type = typeof(T);
        var tableAttr = type.GetCustomAttribute<TableAttribute>();
        var tableName = tableAttr?.Name ?? type.Name.ToLower();
        var properties = type.GetProperties();

        var pkProperty = properties.FirstOrDefault(p => p.GetCustomAttribute<PrimaryKeyAttribute>() != null);
        if (pkProperty == null)
            return null;

        var pkColumnAttr = pkProperty.GetCustomAttribute<ColumnAttribute>();
        var pkColumn = pkColumnAttr?.Name ?? pkProperty.Name.ToLower();

        var columns = properties
            .Where(p => !IsNavigationProperty(p))
            .Select(p =>
            {
                var colAttr = p.GetCustomAttribute<ColumnAttribute>();
                return colAttr?.Name ?? p.Name.ToLower();
            });

        var sql = $"SELECT {string.Join(", ", columns)} FROM {tableName} WHERE {pkColumn} = @id";

        using var cmd = new NpgsqlCommand(sql, _context.GetConnection());
        cmd.Parameters.AddWithValue("@id", _foreignKeyValue);
        using var reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            var entity = new T();
            var nonNavProperties = properties.Where(p => !IsNavigationProperty(p)).ToArray();
            for (int i = 0; i < nonNavProperties.Length; i++)
            {
                var value = reader.GetValue(i);
                if (value != DBNull.Value)
                {
                    nonNavProperties[i].SetValue(entity, value);
                }
            }
            return entity;
        }
        return null;
    }

    private bool IsNavigationProperty(PropertyInfo prop)
    {
        var propType = prop.PropertyType;
        if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(LazyLoader<>))
            return true;
        if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(List<>))
            return true;
        return false;
    }
}

public class LazyCollection<T> where T : class, new()
{
    private readonly DbContext _context;
    private readonly string _foreignKeyColumn;
    private readonly object _foreignKeyValue;
    private List<T>? _items;
    private bool _loaded;

    public LazyCollection(DbContext context, string foreignKeyColumn, object foreignKeyValue)
    {
        _context = context;
        _foreignKeyColumn = foreignKeyColumn;
        _foreignKeyValue = foreignKeyValue;
        _loaded = false;
    }

    public List<T> Items
    {
        get
        {
            if (!_loaded)
            {
                _items = Load();
                _loaded = true;
            }
            return _items ?? new List<T>();
        }
    }

    private List<T> Load()
    {
        var results = new List<T>();
        var type = typeof(T);
        var tableAttr = type.GetCustomAttribute<TableAttribute>();
        var tableName = tableAttr?.Name ?? type.Name.ToLower();
        var properties = type.GetProperties().Where(p => !IsNavigationProperty(p)).ToArray();

        var columns = properties.Select(p =>
        {
            var colAttr = p.GetCustomAttribute<ColumnAttribute>();
            return colAttr?.Name ?? p.Name.ToLower();
        });

        var sql = $"SELECT {string.Join(", ", columns)} FROM {tableName} WHERE {_foreignKeyColumn} = @fk";

        using var cmd = new NpgsqlCommand(sql, _context.GetConnection());
        cmd.Parameters.AddWithValue("@fk", _foreignKeyValue);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var entity = new T();
            for (int i = 0; i < properties.Length; i++)
            {
                var value = reader.GetValue(i);
                if (value != DBNull.Value)
                {
                    properties[i].SetValue(entity, value);
                }
            }
            results.Add(entity);
        }
        return results;
    }

    private bool IsNavigationProperty(PropertyInfo prop)
    {
        var propType = prop.PropertyType;
        if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(LazyLoader<>))
            return true;
        if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(LazyCollection<>))
            return true;
        return false;
    }
}
