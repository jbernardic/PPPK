namespace ORM.Migrations;

public class M004_CreateMedicalHistoriesTable : Migration
{
    public override string Version => "004";
    public override string Description => "Create medical_histories table";

    public override string Up()
    {
        return @"
            CREATE TABLE IF NOT EXISTS medical_histories (
                id SERIAL PRIMARY KEY,
                patient_id INTEGER NOT NULL REFERENCES patients(id) ON DELETE CASCADE,
                disease_name TEXT NOT NULL,
                start_date TIMESTAMP NOT NULL,
                end_date TIMESTAMP
            );";
    }

    public override string Down()
    {
        return "DROP TABLE IF EXISTS medical_histories;";
    }
}
