using System.ComponentModel.DataAnnotations;

namespace KosnicaApi.Models;

public class Intervention
{
    public int Id { get; set; }
    
    public int? HiveId { get; set; }
    public Hive? Hive { get; set; }

    public int? ApiaryId { get; set; }
    public Apiary? Apiary { get; set; }

    public required InterventionType Type { get; set; } // e.g. Spring review, wintering

    public string? Description { get; set; }

    public DateTime PlannedDate { get; set; }
    public DateTime? ExecutionDate { get; set; }
    
    public InterventionStatus Status { get; set; } = InterventionStatus.Planned;

    // Navigation property
    public ICollection<Treatment> Treatments { get; set; } = new List<Treatment>();
}
