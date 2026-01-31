namespace ORM.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ForeignKeyAttribute : Attribute
{
    public string Name { get; }
    public Type ReferencedType { get; }

    public ForeignKeyAttribute(string name, Type referencedType)
    {
        Name = name;
        ReferencedType = referencedType;
    }
}
