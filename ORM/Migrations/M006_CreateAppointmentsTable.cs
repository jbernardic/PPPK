namespace ORM.Migrations;

public class M006_CreateAppointmentsTable : Migration
{
    public override string Version => "006";
    public override string Description => "Create appointments table";

    public override string Up()
    {
        return @"
            CREATE TABLE IF NOT EXISTS appointments (
                id SERIAL PRIMARY KEY,
                patient_id INTEGER NOT NULL REFERENCES patients(id) ON DELETE CASCADE,
                doctor_id INTEGER NOT NULL REFERENCES doctors(id),
                examination_type VARCHAR(10) NOT NULL CHECK (examination_type IN ('CT', 'MR', 'ULTRA', 'EKG', 'ECHO', 'OKO', 'DERM', 'DENTA', 'MAMMO', 'EEG')),
                scheduled_at TIMESTAMP NOT NULL,
                notes TEXT
            );";
    }

    public override string Down()
    {
        return "DROP TABLE IF EXISTS appointments;";
    }
}
