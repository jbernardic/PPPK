using ORM.Attributes;
using ORM.Core;

namespace ORM.Models;

[Table("appointments")]
public class Appointment
{
    [PrimaryKey]
    [Column("id")]
    public int Id { get; set; }

    [Column("patient_id")]
    [ForeignKey("patient_id", typeof(Patient))]
    public int PatientId { get; set; }

    [Column("doctor_id")]
    [ForeignKey("doctor_id", typeof(Doctor))]
    public int DoctorId { get; set; }

    [Column("examination_type")]
    public string ExaminationType { get; set; } = string.Empty;

    [Column("scheduled_at")]
    public DateTime ScheduledAt { get; set; }

    [Column("notes")]
    public string Notes { get; set; } = string.Empty;

    public LazyLoader<Patient>? Patient { get; set; }
    public LazyLoader<Doctor>? Doctor { get; set; }

    public void SetContext(DbContext context)
    {
        Patient = new LazyLoader<Patient>(context, PatientId);
        Doctor = new LazyLoader<Doctor>(context, DoctorId);
    }
}
