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
public class InterventionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public InterventionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InterventionDto>>> GetInterventions([FromQuery] int? apiaryId, [FromQuery] int? hiveId)
    {
        var ownerId = GetEffectiveOwnerId();

        var query = _context.Interventions.AsQueryable();

        // Enforce user ownership
        if (apiaryId.HasValue)
        {
            if (!await _context.Apiaries.AnyAsync(a => a.Id == apiaryId && a.UserId == ownerId))
            {
                return Forbid();
            }
            // Include interventions linked directly to apiary OR linked to hives within that apiary
            query = query.Where(i => i.ApiaryId == apiaryId || (i.Hive != null && i.Hive.ApiaryId == apiaryId));
        }
        else if (hiveId.HasValue)
        {
            if (!await _context.Hives.Include(h => h.Apiary).AnyAsync(h => h.Id == hiveId && h.Apiary!.UserId == ownerId))
            {
                return Forbid();
            }
            query = query.Where(i => i.HiveId == hiveId);
        }
        else
        {
             // If no filter, get all interventions belonging to the user's apiaries/hives
             query = query.Where(i => 
                (i.ApiaryId.HasValue && i.Apiary!.UserId == ownerId) || 
                (i.HiveId.HasValue && i.Hive!.Apiary!.UserId == ownerId));
        }

        return await query.Select(i => new InterventionDto
        {
            Id = i.Id,
            HiveId = i.HiveId,
            ApiaryId = i.ApiaryId,
            Type = i.Type,
            Description = i.Description,
            PlannedDate = i.PlannedDate,
            ExecutionDate = i.ExecutionDate,
            Status = i.Status,
            ApiaryName = i.ApiaryId.HasValue ? i.Apiary!.Name : (i.HiveId.HasValue ? i.Hive!.Apiary!.Name : null),
            HiveIdentifier = i.HiveId.HasValue ? i.Hive!.Identifier : null
        }).ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<InterventionDto>> CreateIntervention(CreateInterventionDto dto)
    {
        var ownerId = GetEffectiveOwnerId();

        // Validate ownership
        if (dto.ApiaryId.HasValue)
        {
            if (!await _context.Apiaries.AnyAsync(a => a.Id == dto.ApiaryId && a.UserId == ownerId))
            {
                return NotFound("Pčelinjak nije pronađen.");
            }
        }
        else if (dto.HiveId.HasValue)
        {
            if (!await _context.Hives.Include(h => h.Apiary).AnyAsync(h => h.Id == dto.HiveId && h.Apiary!.UserId == ownerId))
            {
                 return NotFound("Košnica nije pronađena.");
            }
        }
        else
        {
            return BadRequest("Intervencija mora biti vezana za pčelinjak ili košnicu.");
        }

        var intervention = new Intervention
        {
            HiveId = dto.HiveId,
            ApiaryId = dto.ApiaryId,
            Type = dto.Type,
            Description = dto.Description,
            PlannedDate = dto.PlannedDate.ToUniversalTime(),
            Status = dto.Status,
            ExecutionDate = dto.Status == InterventionStatus.Completed ? DateTime.UtcNow : null
        };

        _context.Interventions.Add(intervention);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetInterventions), new { id = intervention.Id }, new InterventionDto
        {
            Id = intervention.Id,
            HiveId = intervention.HiveId,
            ApiaryId = intervention.ApiaryId,
            Type = intervention.Type,
            Description = intervention.Description,
            PlannedDate = intervention.PlannedDate,
            ExecutionDate = intervention.ExecutionDate,
            Status = intervention.Status
        });
    }

    [HttpPost("bulk")]
    public async Task<ActionResult<IEnumerable<InterventionDto>>> CreateBulkInterventions(BulkCreateInterventionDto dto)
    {
        var ownerId = GetEffectiveOwnerId();

        // Validate ownership of all hives
        var hives = await _context.Hives
            .Include(h => h.Apiary)
            .Where(h => dto.HiveIds.Contains(h.Id))
            .ToListAsync();

        if (hives.Count != dto.HiveIds.Count)
        {
            return BadRequest("Jedna ili više košnica nisu pronađene.");
        }

        if (hives.Any(h => h.Apiary?.UserId != ownerId))
        {
            return Forbid();
        }

        var interventions = new List<Intervention>();
        var resultDtos = new List<InterventionDto>();

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var hiveId in dto.HiveIds)
            {
                var intervention = new Intervention
                {
                    HiveId = hiveId,
                    Type = dto.Type,
                    Description = dto.Description,
                    PlannedDate = dto.PlannedDate.ToUniversalTime(),
                    Status = dto.Status,
                    ExecutionDate = dto.Status == InterventionStatus.Completed ? DateTime.UtcNow : null
                };
                interventions.Add(intervention);
            }

            _context.Interventions.AddRange(interventions);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            resultDtos.AddRange(interventions.Select(i => new InterventionDto
            {
                Id = i.Id,
                HiveId = i.HiveId,
                ApiaryId = i.ApiaryId,
                Type = i.Type,
                Description = i.Description,
                PlannedDate = i.PlannedDate,
                ExecutionDate = i.ExecutionDate,
                Status = i.Status
            }));

            return Ok(resultDtos);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "Došlo je do greške prilikom kreiranja grupnih intervencija.");
        }
    }

    [HttpPost("bulk-apiary/{apiaryId}")]
    public async Task<ActionResult<IEnumerable<InterventionDto>>> CreateApiaryBulkInterventions(int apiaryId, [FromBody] CreateInterventionDto dto)
    {
        var ownerId = GetEffectiveOwnerId();

        // Verify apiary ownership
        var hives = await _context.Hives
            .Include(h => h.Apiary)
            .Where(h => h.ApiaryId == apiaryId && h.Apiary!.UserId == ownerId)
            .ToListAsync();

        if (hives.Count == 0)
        {
            return NotFound("Nisu pronađene košnice u ovom pčelinjaku.");
        }

        var interventions = new List<Intervention>();
        var resultDtos = new List<InterventionDto>();

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var hive in hives)
            {
                var intervention = new Intervention
                {
                    HiveId = hive.Id,
                    ApiaryId = null, // Link to individual hives
                    Type = dto.Type,
                    Description = dto.Description,
                    PlannedDate = dto.PlannedDate.ToUniversalTime(),
                    Status = dto.Status,
                    ExecutionDate = dto.Status == InterventionStatus.Completed ? DateTime.UtcNow : null
                };
                interventions.Add(intervention);
            }

            _context.Interventions.AddRange(interventions);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            resultDtos.AddRange(interventions.Select(i => new InterventionDto
            {
                Id = i.Id,
                HiveId = i.HiveId,
                ApiaryId = i.ApiaryId,
                Type = i.Type,
                Description = i.Description,
                PlannedDate = i.PlannedDate,
                ExecutionDate = i.ExecutionDate,
                Status = i.Status
            }));

            return Ok(resultDtos);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "Došlo je do greške prilikom kreiranja grupnih intervencija.");
        }
    }

    [HttpPut("{id}/complete")]
    public async Task<IActionResult> CompleteIntervention(int id)
    {
        var ownerId = GetEffectiveOwnerId();

        var intervention = await _context.Interventions
            .Include(i => i.Apiary)
            .Include(i => i.Hive).ThenInclude(h => h!.Apiary)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (intervention == null)
        {
            return NotFound("Intervencija nije pronađena.");
        }

        // Validate Ownership
        bool isOwned = false;
        if (intervention.ApiaryId.HasValue && intervention.Apiary?.UserId == ownerId)
        {
            isOwned = true;
        }
        else if (intervention.HiveId.HasValue && intervention.Hive?.Apiary?.UserId == ownerId)
        {
            isOwned = true;
        }

        if (!isOwned) return Forbid();

        intervention.Status = InterventionStatus.Completed;
        intervention.ExecutionDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateInterventionStatus(int id, [FromBody] InterventionStatus status)
    {
        var ownerId = GetEffectiveOwnerId();

        var intervention = await _context.Interventions
            .Include(i => i.Apiary)
            .Include(i => i.Hive).ThenInclude(h => h!.Apiary)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (intervention == null) return NotFound("Intervencija nije pronađena.");

        // Validate Ownership
        bool isOwned = (intervention.ApiaryId.HasValue && intervention.Apiary?.UserId == ownerId) ||
                       (intervention.HiveId.HasValue && intervention.Hive?.Apiary?.UserId == ownerId);

        if (!isOwned) return Forbid();

        intervention.Status = status;
        if (status == InterventionStatus.Completed)
        {
            intervention.ExecutionDate = DateTime.UtcNow;
        }
        else
        {
            intervention.ExecutionDate = null;
        }

        await _context.SaveChangesAsync();
        return Ok(new InterventionDto
        {
            Id = intervention.Id,
            HiveId = intervention.HiveId,
            ApiaryId = intervention.ApiaryId,
            Type = intervention.Type,
            Description = intervention.Description,
            PlannedDate = intervention.PlannedDate,
            ExecutionDate = intervention.ExecutionDate,
            Status = intervention.Status
        });
    }

    [HttpPatch("{id}/date")]
    public async Task<ActionResult<InterventionDto>> UpdatePlannedDate(int id, UpdateInterventionDateDto dto)
    {
        var ownerId = GetEffectiveOwnerId();
        var intervention = await _context.Interventions
            .Include(i => i.Apiary)
            .Include(i => i.Hive).ThenInclude(h => h!.Apiary)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (intervention == null) return NotFound();

        bool isOwned = (intervention.ApiaryId.HasValue && intervention.Apiary?.UserId == ownerId) ||
                       (intervention.HiveId.HasValue && intervention.Hive?.Apiary?.UserId == ownerId);

        if (!isOwned) return Forbid();

        intervention.PlannedDate = dto.PlannedDate.ToUniversalTime();
        await _context.SaveChangesAsync();

        return Ok(new InterventionDto
        {
            Id = intervention.Id,
            HiveId = intervention.HiveId,
            ApiaryId = intervention.ApiaryId,
            Type = intervention.Type,
            Description = intervention.Description,
            PlannedDate = intervention.PlannedDate,
            ExecutionDate = intervention.ExecutionDate,
            Status = intervention.Status,
            ApiaryName = intervention.ApiaryId.HasValue ? intervention.Apiary!.Name : (intervention.HiveId.HasValue ? intervention.Hive!.Apiary!.Name : null),
            HiveIdentifier = intervention.HiveId.HasValue ? intervention.Hive!.Identifier : null
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Beekeeper")]
    public async Task<IActionResult> DeleteIntervention(int id)
    {
        var ownerId = GetEffectiveOwnerId();

         var intervention = await _context.Interventions
            .Include(i => i.Apiary)
            .Include(i => i.Hive).ThenInclude(h => h!.Apiary)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (intervention == null)
        {
            return NotFound("Intervencija nije pronađena.");
        }

        // Validate Ownership
        bool isOwned = false;
        if (intervention.ApiaryId.HasValue && intervention.Apiary?.UserId == ownerId)
        {
            isOwned = true;
        }
        else if (intervention.HiveId.HasValue && intervention.Hive?.Apiary?.UserId == ownerId)
        {
            isOwned = true;
        }

        if (!isOwned) return Forbid();

        _context.Interventions.Remove(intervention);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private int GetEffectiveOwnerId()
    {
        var employerIdClaim = User.FindFirstValue("EmployerId");
        if (!string.IsNullOrEmpty(employerIdClaim)) return int.Parse(employerIdClaim);

        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
