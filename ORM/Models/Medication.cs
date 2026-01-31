using ORM.Attributes;

namespace ORM.Models;

[Table("medications")]
public class Medication
{
    [PrimaryKey]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("dose_unit")]
    public string DoseUnit { get; set; } = string.Empty;
}
