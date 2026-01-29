using System.Reflection;
using Npgsql;
using ORM.Attributes;

namespace ORM.Core;

public class DbSet<T> where T : class, new()
{
    private readonly DbContext _context;
    private readonly string _tableName;
    private readonly PropertyInfo[] _properties;

    public DbSet(DbContext context)
    {
        _context = context;
        _tableName = GetTableName();
        _properties = typeof(T).GetProperties();
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
        return _properties.FirstOrDefault(p => p.GetCustomAttribute<PrimaryKeyAttribute>() != null);
    }

    public List<T> ToList()
    {
        var results = new List<T>();
        var columns = string.Join(", ", _properties.Select(GetColumnName));
        var sql = $"SELECT {columns} FROM {_tableName}";

        using var cmd = new NpgsqlCommand(sql, _context.GetConnection());
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var entity = new T();
            for (int i = 0; i < _properties.Length; i++)
            {
                var value = reader.GetValue(i);
                if (value != DBNull.Value)
                {
                    _properties[i].SetValue(entity, value);
                }
            }
            results.Add(entity);
        }
        return results;
    }

    public T? Find(object id)
    {
        var pkProperty = GetPrimaryKeyProperty();
        if (pkProperty == null)
            throw new InvalidOperationException("No primary key defined");

        var columns = string.Join(", ", _properties.Select(GetColumnName));
        var pkColumn = GetColumnName(pkProperty);
        var sql = $"SELECT {columns} FROM {_tableName} WHERE {pkColumn} = @id";

        using var cmd = new NpgsqlCommand(sql, _context.GetConnection());
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            var entity = new T();
            for (int i = 0; i < _properties.Length; i++)
            {
                var value = reader.GetValue(i);
                if (value != DBNull.Value)
                {
                    _properties[i].SetValue(entity, value);
                }
            }
            return entity;
        }
        return null;
    }
}
