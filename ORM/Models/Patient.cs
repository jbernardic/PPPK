using ORM.Attributes;
using ORM.Core;

namespace ORM.Models;

[Table("patients")]
public class Patient
{
    [PrimaryKey]
    [Column("id")]
    public int Id { get; set; }

    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;

    [Column("oib")]
    public string OIB { get; set; } = string.Empty;

    [Column("birth_date")]
    public DateTime BirthDate { get; set; }

    [Column("gender")]
    public string Gender { get; set; } = string.Empty;

    [Column("residence_address")]
    public string ResidenceAddress { get; set; } = string.Empty;

    [Column("domicile_address")]
    public string DomicileAddress { get; set; } = string.Empty;

    public LazyCollection<MedicalHistory>? MedicalHistories { get; set; }
    public LazyCollection<Prescription>? Prescriptions { get; set; }
    public LazyCollection<Appointment>? Appointments { get; set; }

    public void SetContext(DbContext context)
    {
        MedicalHistories = new LazyCollection<MedicalHistory>(context, "patient_id", Id);
        Prescriptions = new LazyCollection<Prescription>(context, "patient_id", Id);
        Appointments = new LazyCollection<Appointment>(context, "patient_id", Id);
    }
}
