namespace ORM.Migrations;

public abstract class Migration
{
    public abstract string Version { get; }
    public abstract string Description { get; }
    public abstract string Up();
    public abstract string Down();
}
