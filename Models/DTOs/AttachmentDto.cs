using KosnicaApi.Models;

namespace KosnicaApi.Models.DTOs;

public class UploadAttachmentDto
{
    public int? HiveId { get; set; }
    public int? ApiaryId { get; set; }
    public int? InterventionId { get; set; }
    public int? YieldRecordId { get; set; }
    public AttachmentType Type { get; set; }

    // Handled via [FromForm] in controller directly along with IFormFile, 
    // but represented loosely by this DTO.
}

public class AttachmentDto
{
    public int Id { get; set; }
    public int? HiveId { get; set; }
    public int? ApiaryId { get; set; }
    public int? InterventionId { get; set; }
    public int? YieldRecordId { get; set; }
    public required string FileUrl { get; set; }
    public AttachmentType Type { get; set; }
}
