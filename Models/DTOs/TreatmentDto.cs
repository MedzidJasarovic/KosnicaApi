using System.ComponentModel.DataAnnotations;

namespace KosnicaApi.Models.DTOs;

public class CreateTreatmentDto
{
    [Required]
    public int HiveId { get; set; }
    
    public int? InterventionId { get; set; }

    public DateTime DateApplied { get; set; }

    [Required]
    [MaxLength(100)]
    public required string SubstanceName { get; set; }

    [MaxLength(50)]
    public string? Dose { get; set; }

    public string? BeeReaction { get; set; }
    public string? Losses { get; set; }
}

public class BulkCreateTreatmentDto
{
    [Required]
    public List<int> HiveIds { get; set; } = new();

    public int? InterventionId { get; set; }

    public DateTime DateApplied { get; set; }

    [Required]
    [MaxLength(100)]
    public required string SubstanceName { get; set; }

    [MaxLength(50)]
    public string? Dose { get; set; }

    public string? BeeReaction { get; set; }
    public string? Losses { get; set; }
}

public class TreatmentDto
{
    public int Id { get; set; }
    public int HiveId { get; set; }
    public int? InterventionId { get; set; }
    public DateTime DateApplied { get; set; }
    public required string SubstanceName { get; set; }
    public string? Dose { get; set; }
    public string? BeeReaction { get; set; }
    public string? Losses { get; set; }
    public string? ApiaryName { get; set; }
    public string? HiveIdentifier { get; set; }
}
