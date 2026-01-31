namespace ORM.Migrations;

public class M003_CreateMedicationsTable : Migration
{
    public override string Version => "003";
    public override string Description => "Create medications table";

    public override string Up()
    {
        return @"
            CREATE TABLE IF NOT EXISTS medications (
                id SERIAL PRIMARY KEY,
                name TEXT NOT NULL,
                dose_unit TEXT NOT NULL
            );";
    }

    public override string Down()
    {
        return "DROP TABLE IF EXISTS medications;";
    }
}
