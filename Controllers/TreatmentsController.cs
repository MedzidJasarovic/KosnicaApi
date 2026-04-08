using System.Security.Claims;
using KosnicaApi.Data;
using KosnicaApi.Models;
using KosnicaApi.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KosnicaApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TreatmentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TreatmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    private int GetEffectiveOwnerId()
    {
        var employerIdClaim = User.FindFirstValue("EmployerId");
        if (!string.IsNullOrEmpty(employerIdClaim)) return int.Parse(employerIdClaim);

        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TreatmentDto>>> GetTreatments([FromQuery] int? apiaryId, [FromQuery] int? hiveId)
    {
        var ownerId = GetEffectiveOwnerId();

        var query = _context.Treatments.AsQueryable();

        if (apiaryId.HasValue)
        {
            if (!await _context.Apiaries.AnyAsync(a => a.Id == apiaryId && a.UserId == ownerId)) return Forbid();
            query = query.Where(t => t.Hive!.ApiaryId == apiaryId);
        }
        else if (hiveId.HasValue)
        {
            if (!await _context.Hives.Include(h => h.Apiary).AnyAsync(h => h.Id == hiveId && h.Apiary!.UserId == ownerId)) return Forbid();
            query = query.Where(t => t.HiveId == hiveId);
        }
        else
        {
            query = query.Where(t => t.Hive!.Apiary!.UserId == ownerId);
        }

        return await query.Select(t => new TreatmentDto
        {
            Id = t.Id,
            HiveId = t.HiveId,
            InterventionId = t.InterventionId,
            DateApplied = t.DateApplied,
            SubstanceName = t.SubstanceName,
            Dose = t.Dose,
            BeeReaction = t.BeeReaction,
            Losses = t.Losses,
            ApiaryName = t.Hive!.Apiary!.Name,
            HiveIdentifier = t.Hive!.Identifier
        }).ToListAsync();
    }

    [HttpPost]
    [Authorize(Roles = "Beekeeper,Veterinarian")]
    public async Task<ActionResult<TreatmentDto>> CreateTreatment(CreateTreatmentDto dto)
    {
        var ownerId = GetEffectiveOwnerId();

        var hive = await _context.Hives.Include(h => h.Apiary).FirstOrDefaultAsync(h => h.Id == dto.HiveId);
        if (hive == null || hive.Apiary?.UserId != ownerId)
        {
            return NotFound("Košnica nije pronađena.");
        }

        var treatment = new Treatment
        {
            HiveId = dto.HiveId,
            InterventionId = dto.InterventionId,
            DateApplied = dto.DateApplied.ToUniversalTime(),
            SubstanceName = dto.SubstanceName,
            Dose = dto.Dose,
            BeeReaction = dto.BeeReaction,
            Losses = dto.Losses
        };

        _context.Treatments.Add(treatment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTreatments), new { hiveId = treatment.HiveId }, new TreatmentDto
        {
            Id = treatment.Id,
            HiveId = treatment.HiveId,
            InterventionId = treatment.InterventionId,
            DateApplied = treatment.DateApplied,
            SubstanceName = treatment.SubstanceName,
            Dose = treatment.Dose,
            BeeReaction = treatment.BeeReaction,
            Losses = treatment.Losses
        });
    }

    [HttpPost("bulk")]
    [Authorize(Roles = "Beekeeper,Veterinarian")]
    public async Task<ActionResult<IEnumerable<TreatmentDto>>> CreateBulkTreatments(BulkCreateTreatmentDto dto)
    {
        var ownerId = GetEffectiveOwnerId();

        var hives = await _context.Hives
            .Include(h => h.Apiary)
            .Where(h => dto.HiveIds.Contains(h.Id))
            .ToListAsync();

        if (hives.Count != dto.HiveIds.Count || hives.Any(h => h.Apiary?.UserId != ownerId))
        {
            return BadRequest("Jedna ili više košnica nisu pronađene ili nemate pravo pristupa.");
        }

        var treatments = new List<Treatment>();
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var hiveId in dto.HiveIds)
            {
                treatments.Add(new Treatment
                {
                    HiveId = hiveId,
                    InterventionId = dto.InterventionId,
                    DateApplied = dto.DateApplied.ToUniversalTime(),
                    SubstanceName = dto.SubstanceName,
                    Dose = dto.Dose,
                    BeeReaction = dto.BeeReaction,
                    Losses = dto.Losses
                });
            }

            _context.Treatments.AddRange(treatments);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(treatments.Select(t => new TreatmentDto
            {
                Id = t.Id,
                HiveId = t.HiveId,
                InterventionId = t.InterventionId,
                DateApplied = t.DateApplied,
                SubstanceName = t.SubstanceName,
                Dose = t.Dose,
                BeeReaction = t.BeeReaction,
                Losses = t.Losses
            }));
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "Došlo je do greške prilikom unosa grupnih tretmana.");
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Beekeeper,Veterinarian")]
    public async Task<IActionResult> DeleteTreatment(int id)
    {
        var ownerId = GetEffectiveOwnerId();

        var treatment = await _context.Treatments
            .Include(t => t.Hive)
            .ThenInclude(h => h!.Apiary)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (treatment == null || treatment.Hive?.Apiary?.UserId != ownerId)
        {
            return NotFound("Tretman nije pronađen.");
        }

        _context.Treatments.Remove(treatment);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
