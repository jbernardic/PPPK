using System.Reflection;
using Npgsql;
using ORM.Attributes;

namespace ORM.Core;

public class DbSet<T> where T : class, new()
{
    private readonly DbContext _context;
    private readonly string _tableName;
    private readonly PropertyInfo[] _properties;
    private readonly PropertyInfo[] _columnProperties;

    public DbSet(DbContext context)
    {
        _context = context;
        _tableName = GetTableName();
        _properties = typeof(T).GetProperties();
        _columnProperties = _properties.Where(p => !IsNavigationProperty(p)).ToArray();
    }

    private string GetTableName()
    {
        var tableAttr = typeof(T).GetCustomAttribute<TableAttribute>();
        return tableAttr?.Name ?? typeof(T).Name.ToLower();
    }

    private string GetColumnName(PropertyInfo property)
    {
        var columnAttr = property.GetCustomAttribute<ColumnAttribute>();
        return columnAttr?.Name ?? property.Name.ToLower();
    }

    private PropertyInfo? GetPrimaryKeyProperty()
    {
        return _columnProperties.FirstOrDefault(p => p.GetCustomAttribute<PrimaryKeyAttribute>() != null);
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

    private void InitializeLazyLoading(T entity)
    {
        var setContextMethod = typeof(T).GetMethod("SetContext");
        if (setContextMethod != null)
        {
            setContextMethod.Invoke(entity, new object[] { _context });
        }
    }

    public List<T> ToList()
    {
        var results = new List<T>();
        var columns = string.Join(", ", _columnProperties.Select(GetColumnName));
        var sql = $"SELECT {columns} FROM {_tableName}";

        using var cmd = new NpgsqlCommand(sql, _context.GetConnection());
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var entity = new T();
            for (int i = 0; i < _columnProperties.Length; i++)
            {
                var value = reader.GetValue(i);
                if (value != DBNull.Value)
                {
                    _columnProperties[i].SetValue(entity, value);
                }
            }
            results.Add(entity);
        }

        foreach (var entity in results)
        {
            InitializeLazyLoading(entity);
        }

        return results;
    }

    public T? Find(object id)
    {
        var pkProperty = GetPrimaryKeyProperty();
        if (pkProperty == null)
            throw new InvalidOperationException("No primary key defined");

        var columns = string.Join(", ", _columnProperties.Select(GetColumnName));
        var pkColumn = GetColumnName(pkProperty);
        var sql = $"SELECT {columns} FROM {_tableName} WHERE {pkColumn} = @id";

        using var cmd = new NpgsqlCommand(sql, _context.GetConnection());
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            var entity = new T();
            for (int i = 0; i < _columnProperties.Length; i++)
            {
                var value = reader.GetValue(i);
                if (value != DBNull.Value)
                {
                    _columnProperties[i].SetValue(entity, value);
                }
            }
            InitializeLazyLoading(entity);
            return entity;
        }
        return null;
    }

    public void Add(T entity)
    {
        var columns = _columnProperties
            .Where(p => p.GetCustomAttribute<PrimaryKeyAttribute>() == null)
            .ToArray();

        var columnNames = string.Join(", ", columns.Select(GetColumnName));
        var paramNames = string.Join(", ", columns.Select((_, i) => $"@p{i}"));

        var sql = $"INSERT INTO {_tableName} ({columnNames}) VALUES ({paramNames}) RETURNING {GetColumnName(GetPrimaryKeyProperty()!)}";

        using var cmd = new NpgsqlCommand(sql, _context.GetConnection());
        for (int i = 0; i < columns.Length; i++)
        {
            var value = columns[i].GetValue(entity) ?? DBNull.Value;
            cmd.Parameters.AddWithValue($"@p{i}", value);
        }

        var id = cmd.ExecuteScalar();
        if (id != null)
        {
            GetPrimaryKeyProperty()?.SetValue(entity, id);
        }
        InitializeLazyLoading(entity);
    }

    public void Update(T entity)
    {
        var pkProperty = GetPrimaryKeyProperty();
        if (pkProperty == null)
            throw new InvalidOperationException("No primary key defined");

        var columns = _columnProperties
            .Where(p => p.GetCustomAttribute<PrimaryKeyAttribute>() == null)
            .ToArray();

        var setClauses = columns.Select((p, i) => $"{GetColumnName(p)} = @p{i}");
        var sql = $"UPDATE {_tableName} SET {string.Join(", ", setClauses)} WHERE {GetColumnName(pkProperty)} = @pk";

        using var cmd = new NpgsqlCommand(sql, _context.GetConnection());
        for (int i = 0; i < columns.Length; i++)
        {
            var value = columns[i].GetValue(entity) ?? DBNull.Value;
            cmd.Parameters.AddWithValue($"@p{i}", value);
        }
        cmd.Parameters.AddWithValue("@pk", pkProperty.GetValue(entity)!);

        cmd.ExecuteNonQuery();
    }

    public void Delete(T entity)
    {
        var pkProperty = GetPrimaryKeyProperty();
        if (pkProperty == null)
            throw new InvalidOperationException("No primary key defined");

        var sql = $"DELETE FROM {_tableName} WHERE {GetColumnName(pkProperty)} = @pk";

        using var cmd = new NpgsqlCommand(sql, _context.GetConnection());
        cmd.Parameters.AddWithValue("@pk", pkProperty.GetValue(entity)!);

        cmd.ExecuteNonQuery();
    }

    public void Delete(object id)
    {
        var pkProperty = GetPrimaryKeyProperty();
        if (pkProperty == null)
            throw new InvalidOperationException("No primary key defined");

        var sql = $"DELETE FROM {_tableName} WHERE {GetColumnName(pkProperty)} = @pk";

        using var cmd = new NpgsqlCommand(sql, _context.GetConnection());
        cmd.Parameters.AddWithValue("@pk", id);

        cmd.ExecuteNonQuery();
    }
}
