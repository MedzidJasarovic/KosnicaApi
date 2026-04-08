namespace KosnicaApi.Models.DTOs;

public class CreateInterventionDto
{
    public int? HiveId { get; set; }
    public int? ApiaryId { get; set; }
    public required InterventionType Type { get; set; }
    public string? Description { get; set; }
    public DateTime PlannedDate { get; set; }
    public InterventionStatus Status { get; set; }
}

public class InterventionDto
{
    public int Id { get; set; }
    public int? HiveId { get; set; }
    public int? ApiaryId { get; set; }
    public required InterventionType Type { get; set; }
    public string? Description { get; set; }
    public DateTime PlannedDate { get; set; }
    public DateTime? ExecutionDate { get; set; }
    public InterventionStatus Status { get; set; }
    public string? ApiaryName { get; set; }
    public string? HiveIdentifier { get; set; }
}

public class BulkCreateInterventionDto
{
    public required List<int> HiveIds { get; set; }
    public required InterventionType Type { get; set; }
    public string? Description { get; set; }
    public DateTime PlannedDate { get; set; }
    public InterventionStatus Status { get; set; }
}

public class UpdateInterventionDateDto
{
    public DateTime PlannedDate { get; set; }
}
