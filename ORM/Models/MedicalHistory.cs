using ORM.Attributes;
using ORM.Core;

namespace ORM.Models;

[Table("medical_histories")]
public class MedicalHistory
{
    [PrimaryKey]
    [Column("id")]
    public int Id { get; set; }

    [Column("patient_id")]
    [ForeignKey("patient_id", typeof(Patient))]
    public int PatientId { get; set; }

    [Column("disease_name")]
    public string DiseaseName { get; set; } = string.Empty;

    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    public DateTime? EndDate { get; set; }

    public LazyLoader<Patient>? Patient { get; set; }

    public void SetContext(DbContext context)
    {
        Patient = new LazyLoader<Patient>(context, PatientId);
    }
}
