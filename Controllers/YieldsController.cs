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
public class YieldsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public YieldsController(ApplicationDbContext context)
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
    public async Task<ActionResult<IEnumerable<YieldDto>>> GetYields([FromQuery] int? apiaryId, [FromQuery] int? hiveId, [FromQuery] ProductType? productType)
    {
        var ownerId = GetEffectiveOwnerId();

        var query = _context.YieldRecords.AsQueryable();

        if (apiaryId.HasValue)
        {
            if (!await _context.Apiaries.AnyAsync(a => a.Id == apiaryId && a.UserId == ownerId)) return Forbid();
            query = query.Where(y => y.ApiaryId == apiaryId);
        }
        else if (hiveId.HasValue)
        {
            if (!await _context.Hives.Include(h => h.Apiary).AnyAsync(h => h.Id == hiveId && h.Apiary!.UserId == ownerId)) return Forbid();
            query = query.Where(y => y.HiveId == hiveId);
        }
        else
        {
            // Only get owned records
            query = query.Where(y => 
                (y.ApiaryId.HasValue && y.Apiary!.UserId == ownerId) || 
                (y.HiveId.HasValue && y.Hive!.Apiary!.UserId == ownerId));
        }

        if (productType.HasValue)
        {
            query = query.Where(y => y.ProductType == productType);
        }

        return await query.Select(y => new YieldDto
        {
            Id = y.Id,
            HiveId = y.HiveId,
            ApiaryId = y.ApiaryId,
            Date = y.Date,
            ProductType = y.ProductType,
            Quantity = y.Quantity,
            Notes = y.Notes,
            WeatherConditions = y.WeatherConditions,
            ApiaryName = y.ApiaryId.HasValue ? y.Apiary!.Name : (y.HiveId.HasValue ? y.Hive!.Apiary!.Name : null),
            HiveIdentifier = y.HiveId.HasValue ? y.Hive!.Identifier : null
        }).ToListAsync();
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<IEnumerable<YieldStatisticsDto>>> GetYieldStatistics([FromQuery] int? apiaryId, [FromQuery] int? hiveId)
    {
        var ownerId = GetEffectiveOwnerId();
        
        var query = _context.YieldRecords.AsQueryable();

        if (apiaryId.HasValue)
        {
            if (!await _context.Apiaries.AnyAsync(a => a.Id == apiaryId && a.UserId == ownerId)) return Forbid();
            query = query.Where(y => y.ApiaryId == apiaryId);
        }
        else if (hiveId.HasValue)
        {
            if (!await _context.Hives.Include(h => h.Apiary).AnyAsync(h => h.Id == hiveId && h.Apiary!.UserId == ownerId)) return Forbid();
            query = query.Where(y => y.HiveId == hiveId);
        }
        else
        {
            query = query.Where(y => 
                (y.ApiaryId.HasValue && y.Apiary!.UserId == ownerId) || 
                (y.HiveId.HasValue && y.Hive!.Apiary!.UserId == ownerId));
        }

        var stats = await query
            .GroupBy(y => y.ProductType)
            .Select(g => new YieldStatisticsDto
            {
                ProductType = g.Key,
                TotalQuantity = g.Sum(y => y.Quantity)
            })
            .ToListAsync();

        return Ok(stats);
    }

    [HttpPost]
    public async Task<ActionResult<YieldDto>> CreateYield(CreateYieldDto dto)
    {
        var ownerId = GetEffectiveOwnerId();

        if (dto.ApiaryId.HasValue)
        {
            if (!await _context.Apiaries.AnyAsync(a => a.Id == dto.ApiaryId && a.UserId == ownerId)) return NotFound("Pčelinjak nije pronađen.");
        }
        else if (dto.HiveId.HasValue)
        {
            if (!await _context.Hives.Include(h => h.Apiary).AnyAsync(h => h.Id == dto.HiveId && h.Apiary!.UserId == ownerId)) return NotFound("Košnica nije pronađena.");
        }
        else
        {
            return BadRequest("Prinos mora biti vezan za pčelinjak ili košnicu.");
        }

        var yieldRecord = new YieldRecord
        {
            HiveId = dto.HiveId,
            ApiaryId = dto.ApiaryId,
            Date = dto.Date.ToUniversalTime(),
            ProductType = dto.ProductType,
            Quantity = dto.Quantity,
            Notes = dto.Notes,
            WeatherConditions = dto.WeatherConditions
        };

        _context.YieldRecords.Add(yieldRecord);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetYields), new { id = yieldRecord.Id }, new YieldDto
        {
            Id = yieldRecord.Id,
            HiveId = yieldRecord.HiveId,
            ApiaryId = yieldRecord.ApiaryId,
            Date = yieldRecord.Date,
            ProductType = yieldRecord.ProductType,
            Quantity = yieldRecord.Quantity,
            Notes = yieldRecord.Notes,
            WeatherConditions = yieldRecord.WeatherConditions
        });
    }

    [HttpPost("bulk")]
    public async Task<ActionResult<IEnumerable<YieldDto>>> CreateYieldsBulk([FromBody] IEnumerable<CreateYieldDto> dtos)
    {
        var dtosList = dtos.ToList();
        if(!dtosList.Any()) return BadRequest("Nema prinosa za dodavanje.");

        var ownerId = GetEffectiveOwnerId();
        var createdYields = new List<YieldRecord>();

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var dto in dtosList)
            {
                if (dto.ApiaryId.HasValue)
                {
                    if (!await _context.Apiaries.AnyAsync(a => a.Id == dto.ApiaryId && a.UserId == ownerId)) 
                        return NotFound($"Pčelinjak sa ID {dto.ApiaryId} nije pronađen.");
                }
                else if (dto.HiveId.HasValue)
                {
                    if (!await _context.Hives.Include(h => h.Apiary).AnyAsync(h => h.Id == dto.HiveId && h.Apiary!.UserId == ownerId)) 
                        return NotFound($"Košnica sa ID {dto.HiveId} nije pronađena.");
                }
                else
                {
                    return BadRequest("Prinos mora biti vezan za pčelinjak ili košnicu.");
                }

                var yieldRecord = new YieldRecord
                {
                    HiveId = dto.HiveId,
                    ApiaryId = dto.ApiaryId,
                    Date = dto.Date.ToUniversalTime(),
                    ProductType = dto.ProductType,
                    Quantity = dto.Quantity,
                    Notes = dto.Notes,
                    WeatherConditions = dto.WeatherConditions
                };

                _context.YieldRecords.Add(yieldRecord);
                createdYields.Add(yieldRecord);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var results = createdYields.Select(y => new YieldDto
            {
                Id = y.Id,
                HiveId = y.HiveId,
                ApiaryId = y.ApiaryId,
                Date = y.Date,
                ProductType = y.ProductType,
                Quantity = y.Quantity,
                Notes = y.Notes,
                WeatherConditions = y.WeatherConditions
            });

            return Ok(results);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "Došlo je do greške prilikom snimanja prinosa.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteYield(int id)
    {
        var ownerId = GetEffectiveOwnerId();

        var yieldRecord = await _context.YieldRecords
            .Include(y => y.Apiary)
            .Include(y => y.Hive).ThenInclude(h => h!.Apiary)
            .FirstOrDefaultAsync(y => y.Id == id);

        if (yieldRecord == null) return NotFound("Zapis o prinosu nije pronađen.");

        bool isOwned = false;
        if (yieldRecord.ApiaryId.HasValue && yieldRecord.Apiary?.UserId == ownerId) isOwned = true;
        if (yieldRecord.HiveId.HasValue && yieldRecord.Hive?.Apiary?.UserId == ownerId) isOwned = true;

        if (!isOwned) return Forbid();

        _context.YieldRecords.Remove(yieldRecord);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
