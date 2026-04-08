using System.ComponentModel.DataAnnotations;

namespace KosnicaApi.Models;

public class Attachment
{
    public int Id { get; set; }

    // This can be polymorphically linked, or using discriminator.
    // For simplicity, we reference specific foreign keys if they apply.
    public int? HiveId { get; set; }
    public Hive? Hive { get; set; }

    public int? ApiaryId { get; set; }
    public Apiary? Apiary { get; set; }

    public int? InterventionId { get; set; }
    public Intervention? Intervention { get; set; }

    public int? YieldRecordId { get; set; }
    public YieldRecord? YieldRecord { get; set; }

    [Required]
    public required string FileUrl { get; set; }

    public AttachmentType Type { get; set; }
}
