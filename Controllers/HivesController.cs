using System.Security.Claims;
using KosnicaApi.Data;
using KosnicaApi.Models;
using KosnicaApi.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KosnicaApi.Controllers;

[Route("api/apiaries/{apiaryId}/hives")]
[ApiController]
[Authorize]
public class HivesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public HivesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<HiveDto>>> GetHives(int apiaryId)
    {
        var ownerId = GetEffectiveOwnerId();
        
        // Ensure the apiary belongs to the owner
        if (!await _context.Apiaries.AnyAsync(a => a.Id == apiaryId && a.UserId == ownerId))
        {
            return NotFound("Pčelinjak nije pronađen.");
        }

        return await _context.Hives
            .Where(h => h.ApiaryId == apiaryId)
            .Select(h => new HiveDto
            {
                Id = h.Id,
                Identifier = h.Identifier,
                Type = h.Type,
                PositionX = h.PositionX,
                PositionY = h.PositionY,
                NumberOfSupers = h.NumberOfSupers,
                QueenStatus = h.QueenStatus,
                ColonyStrength = h.ColonyStrength
            })
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HiveDto>> GetHive(int apiaryId, int id)
    {
        var ownerId = GetEffectiveOwnerId();

        var hive = await _context.Hives
            .Include(h => h.Apiary)
            .Where(h => h.Id == id && h.ApiaryId == apiaryId && h.Apiary!.UserId == ownerId)
            .Select(h => new HiveDto
            {
                Id = h.Id,
                Identifier = h.Identifier,
                Type = h.Type,
                PositionX = h.PositionX,
                PositionY = h.PositionY,
                NumberOfSupers = h.NumberOfSupers,
                QueenStatus = h.QueenStatus,
                ColonyStrength = h.ColonyStrength
            })
            .FirstOrDefaultAsync();

        if (hive == null)
        {
            return NotFound("Košnica nije pronađena.");
        }

        return hive;
    }

    [HttpPost]
    [Authorize(Roles = "Beekeeper")]
    public async Task<ActionResult<HiveDto>> CreateHive(int apiaryId, CreateHiveDto dto)
    {
        var userId = GetEffectiveOwnerId();

        // Ensure the apiary belongs to the user
        if (!await _context.Apiaries.AnyAsync(a => a.Id == apiaryId && a.UserId == userId))
        {
            return NotFound("Pčelinjak nije pronađen.");
        }

        // Ensure the identifier is unique within this apiary
        if (await _context.Hives.AnyAsync(h => h.ApiaryId == apiaryId && h.Identifier == dto.Identifier))
        {
            return BadRequest("Košnica sa ovim nazivom već postoji u ovom pčelinjaku.");
        }

        var hive = new Hive
        {
            Identifier = dto.Identifier,
            Type = dto.Type,
            PositionX = dto.PositionX,
            PositionY = dto.PositionY,
            NumberOfSupers = dto.NumberOfSupers,
            QueenStatus = dto.QueenStatus,
            ColonyStrength = dto.ColonyStrength,
            ApiaryId = apiaryId
        };

        _context.Hives.Add(hive);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetHive), new { apiaryId = apiaryId, id = hive.Id }, new HiveDto
        {
            Id = hive.Id,
            Identifier = hive.Identifier,
            Type = hive.Type,
            PositionX = hive.PositionX,
            PositionY = hive.PositionY,
            NumberOfSupers = hive.NumberOfSupers,
            QueenStatus = hive.QueenStatus,
            ColonyStrength = hive.ColonyStrength
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Beekeeper")]
    public async Task<IActionResult> UpdateHive(int apiaryId, int id, CreateHiveDto dto)
    {
        var userId = GetEffectiveOwnerId();
        
        var hive = await _context.Hives
            .Include(h => h.Apiary)
            .FirstOrDefaultAsync(h => h.Id == id && h.ApiaryId == apiaryId && h.Apiary!.UserId == userId);

        if (hive == null)
        {
            return NotFound("Košnica nije pronađena.");
        }

        hive.Identifier = dto.Identifier;
        hive.Type = dto.Type;
        hive.PositionX = dto.PositionX;
        hive.PositionY = dto.PositionY;
        hive.NumberOfSupers = dto.NumberOfSupers;
        hive.QueenStatus = dto.QueenStatus;
        hive.ColonyStrength = dto.ColonyStrength;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Beekeeper")]
    public async Task<IActionResult> DeleteHive(int apiaryId, int id)
    {
        var userId = GetEffectiveOwnerId();
        
        var hive = await _context.Hives
            .Include(h => h.Apiary)
            .FirstOrDefaultAsync(h => h.Id == id && h.ApiaryId == apiaryId && h.Apiary!.UserId == userId);

        if (hive == null)
        {
            return NotFound("Košnica nije pronađena.");
        }

        _context.Hives.Remove(hive);
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
