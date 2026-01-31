using ORM.Core;
using ORM.Models;

namespace ORM;

public class MedicalContext : DbContext
{
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Medication> Medications => Set<Medication>();
    public DbSet<MedicalHistory> MedicalHistories => Set<MedicalHistory>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    public MedicalContext(string connectionString) : base(connectionString)
    {
    }
}
