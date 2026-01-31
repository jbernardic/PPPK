using ORM.Attributes;
using ORM.Core;

namespace ORM.Models;

[Table("doctors")]
public class Doctor
{
    [PrimaryKey]
    [Column("id")]
    public int Id { get; set; }

    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;

    [Column("specialization")]
    public string Specialization { get; set; } = string.Empty;

    public LazyCollection<Appointment>? Appointments { get; set; }

    public void SetContext(DbContext context)
    {
        Appointments = new LazyCollection<Appointment>(context, "doctor_id", Id);
    }
}
