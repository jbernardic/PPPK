using ORM.Attributes;
using ORM.Core;

namespace ORM.Models;

[Table("prescriptions")]
public class Prescription
{
    [PrimaryKey]
    [Column("id")]
    public int Id { get; set; }

    [Column("patient_id")]
    [ForeignKey("patient_id", typeof(Patient))]
    public int PatientId { get; set; }

    [Column("medication_id")]
    [ForeignKey("medication_id", typeof(Medication))]
    public int MedicationId { get; set; }

    [Column("condition")]
    public string Condition { get; set; } = string.Empty;

    [Column("dose_amount")]
    public decimal DoseAmount { get; set; }

    [Column("frequency")]
    public string Frequency { get; set; } = string.Empty;

    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    public DateTime? EndDate { get; set; }

    public LazyLoader<Patient>? Patient { get; set; }
    public LazyLoader<Medication>? Medication { get; set; }

    public void SetContext(DbContext context)
    {
        Patient = new LazyLoader<Patient>(context, PatientId);
        Medication = new LazyLoader<Medication>(context, MedicationId);
    }
}
