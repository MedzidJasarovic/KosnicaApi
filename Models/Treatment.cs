using System.ComponentModel.DataAnnotations;

namespace KosnicaApi.Models;

public class Treatment
{
    public int Id { get; set; }
    
    public int HiveId { get; set; }
    public Hive? Hive { get; set; }

    public int? InterventionId { get; set; }
    public Intervention? Intervention { get; set; }

    public DateTime DateApplied { get; set; }

    [Required]
    [MaxLength(100)]
    public required string SubstanceName { get; set; }

    [MaxLength(50)]
    public string? Dose { get; set; }

    public string? BeeReaction { get; set; }
    public string? Losses { get; set; }
}
