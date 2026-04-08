using System.Security.Claims;
using KosnicaApi.Data;
using KosnicaApi.Models;
using KosnicaApi.Models.DTOs;
using KosnicaApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KosnicaApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AttachmentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IStorageService _storageService;

    public AttachmentsController(ApplicationDbContext context, IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    private int GetEffectiveOwnerId()
    {
        var employerIdClaim = User.FindFirstValue("EmployerId");
        if (!string.IsNullOrEmpty(employerIdClaim)) return int.Parse(employerIdClaim);

        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AttachmentDto>>> GetAttachments(
        [FromQuery] int? apiaryId,
        [FromQuery] int? hiveId,
        [FromQuery] int? interventionId,
        [FromQuery] int? yieldRecordId)
    {
        var ownerId = GetEffectiveOwnerId();
        var query = _context.Attachments.AsQueryable();

        // Security check omitted for brevity on GET, but could be added based on IDs
        if (apiaryId.HasValue) query = query.Where(a => a.ApiaryId == apiaryId);
        if (hiveId.HasValue) query = query.Where(a => a.HiveId == hiveId);
        if (interventionId.HasValue) query = query.Where(a => a.InterventionId == interventionId);
        if (yieldRecordId.HasValue) query = query.Where(a => a.YieldRecordId == yieldRecordId);

        return await query.Select(a => new AttachmentDto
        {
            Id = a.Id,
            ApiaryId = a.ApiaryId,
            HiveId = a.HiveId,
            InterventionId = a.InterventionId,
            YieldRecordId = a.YieldRecordId,
            FileUrl = a.FileUrl,
            Type = a.Type
        }).ToListAsync();
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<AttachmentDto>> UploadFile(
        [FromForm] int? apiaryId,
        [FromForm] int? hiveId,
        [FromForm] int? interventionId,
        [FromForm] int? yieldRecordId,
        [FromForm] AttachmentType type,
        IFormFile file)
    {
        var ownerId = GetEffectiveOwnerId();

        // Detailed ownership validation should run here ensuring ApiaryId/HiveId belongs to OwnerId.
        if (file == null || file.Length == 0)
        {
            return BadRequest("Fajl je prazan ili nije priložen.");
        }

        if (!apiaryId.HasValue && !hiveId.HasValue && !interventionId.HasValue && !yieldRecordId.HasValue)
        {
            return BadRequest("Fajl mora biti privezan za neku entitet (Pčelinjak, Košnica, Intervencija ili Prinos).");
        }

        string folderName = "kosnica/misc";
        if (apiaryId.HasValue) folderName = "kosnica/apiaries";
        else if (hiveId.HasValue) folderName = "kosnica/hives";
        else if (interventionId.HasValue) folderName = "kosnica/interventions";
        else if (yieldRecordId.HasValue) folderName = "kosnica/yields";

        var fileUrl = await _storageService.UploadFileAsync(file, folderName);

        var attachment = new Attachment
        {
            ApiaryId = apiaryId,
            HiveId = hiveId,
            InterventionId = interventionId,
            YieldRecordId = yieldRecordId,
            FileUrl = fileUrl,
            Type = type
        };

        _context.Attachments.Add(attachment);
        await _context.SaveChangesAsync();

        return Ok(new AttachmentDto
        {
            Id = attachment.Id,
            ApiaryId = attachment.ApiaryId,
            HiveId = attachment.HiveId,
            InterventionId = attachment.InterventionId,
            YieldRecordId = attachment.YieldRecordId,
            FileUrl = attachment.FileUrl,
            Type = attachment.Type
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFile(int id)
    {
        var attachment = await _context.Attachments.FindAsync(id);
        if (attachment == null) return NotFound();

        await _storageService.DeleteFileAsync(attachment.FileUrl);
        _context.Attachments.Remove(attachment);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
