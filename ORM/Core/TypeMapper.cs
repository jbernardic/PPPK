namespace ORM.Core;

public static class TypeMapper
{
    public static string GetPostgreSqlType(Type clrType)
    {
        var underlyingType = Nullable.GetUnderlyingType(clrType);
        if (underlyingType != null)
            return GetPostgreSqlType(underlyingType);

        return clrType switch
        {
            Type t when t == typeof(int) => "INTEGER",
            Type t when t == typeof(long) => "BIGINT",
            Type t when t == typeof(short) => "SMALLINT",
            Type t when t == typeof(string) => "TEXT",
            Type t when t == typeof(bool) => "BOOLEAN",
            Type t when t == typeof(DateTime) => "TIMESTAMP",
            Type t when t == typeof(decimal) => "DECIMAL",
            Type t when t == typeof(double) => "DOUBLE PRECISION",
            Type t when t == typeof(float) => "REAL",
            Type t when t == typeof(Guid) => "UUID",
            Type t when t == typeof(byte[]) => "BYTEA",
            _ => "TEXT"
        };
    }
}
