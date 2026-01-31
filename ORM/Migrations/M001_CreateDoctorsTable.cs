namespace ORM.Migrations;

public class M001_CreateDoctorsTable : Migration
{
    public override string Version => "001";
    public override string Description => "Create doctors table";

    public override string Up()
    {
        return @"
            CREATE TABLE IF NOT EXISTS doctors (
                id SERIAL PRIMARY KEY,
                first_name TEXT NOT NULL,
                last_name TEXT NOT NULL,
                specialization TEXT NOT NULL
            );";
    }

    public override string Down()
    {
        return "DROP TABLE IF EXISTS doctors;";
    }
}
