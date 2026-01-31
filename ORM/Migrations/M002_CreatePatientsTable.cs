namespace ORM.Migrations;

public class M002_CreatePatientsTable : Migration
{
    public override string Version => "002";
    public override string Description => "Create patients table";

    public override string Up()
    {
        return @"
            CREATE TABLE IF NOT EXISTS patients (
                id SERIAL PRIMARY KEY,
                first_name TEXT NOT NULL,
                last_name TEXT NOT NULL,
                oib VARCHAR(11) UNIQUE NOT NULL,
                birth_date TIMESTAMP NOT NULL,
                gender VARCHAR(10) NOT NULL,
                residence_address TEXT,
                domicile_address TEXT
            );";
    }

    public override string Down()
    {
        return "DROP TABLE IF EXISTS patients;";
    }
}
