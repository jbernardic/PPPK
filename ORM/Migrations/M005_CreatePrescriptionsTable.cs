namespace ORM.Migrations;

public class M005_CreatePrescriptionsTable : Migration
{
    public override string Version => "005";
    public override string Description => "Create prescriptions table";

    public override string Up()
    {
        return @"
            CREATE TABLE IF NOT EXISTS prescriptions (
                id SERIAL PRIMARY KEY,
                patient_id INTEGER NOT NULL REFERENCES patients(id) ON DELETE CASCADE,
                medication_id INTEGER NOT NULL REFERENCES medications(id),
                condition TEXT NOT NULL,
                dose_amount DECIMAL NOT NULL,
                frequency TEXT NOT NULL,
                start_date TIMESTAMP NOT NULL,
                end_date TIMESTAMP
            );";
    }

    public override string Down()
    {
        return "DROP TABLE IF EXISTS prescriptions;";
    }
}
