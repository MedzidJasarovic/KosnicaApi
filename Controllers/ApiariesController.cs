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
public class ApiariesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ApiariesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ApiaryDto>>> GetApiaries()
    {
        var ownerId = GetEffectiveOwnerId();
        
        return await _context.Apiaries
            .Where(a => a.UserId == ownerId)
            .Select(a => new ApiaryDto
            {
                Id = a.Id,
                Name = a.Name,
                Latitude = a.Latitude,
                Longitude = a.Longitude,
                HiveCount = a.Hives.Count
            })
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiaryDto>> GetApiary(int id)
    {
        var ownerId = GetEffectiveOwnerId();
        var apiary = await _context.Apiaries
            .Where(a => a.UserId == ownerId && a.Id == id)
            .Select(a => new ApiaryDto
            {
                Id = a.Id,
                Name = a.Name,
                Latitude = a.Latitude,
                Longitude = a.Longitude,
                HiveCount = a.Hives.Count
            })
            .FirstOrDefaultAsync();

        if (apiary == null)
        {
            return NotFound("Pčelinjak nije pronađen.");
        }

        return apiary;
    }

    [HttpPost]
    [Authorize(Roles = "Beekeeper")]
    public async Task<ActionResult<ApiaryDto>> CreateApiary(CreateApiaryDto dto)
    {
        var userId = GetEffectiveOwnerId();

        var apiary = new Apiary
        {
            Name = dto.Name,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            UserId = userId
        };

        _context.Apiaries.Add(apiary);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetApiary), new { id = apiary.Id }, new ApiaryDto
        {
            Id = apiary.Id,
            Name = apiary.Name,
            Latitude = apiary.Latitude,
            Longitude = apiary.Longitude,
            HiveCount = 0
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Beekeeper")]
    public async Task<IActionResult> UpdateApiary(int id, CreateApiaryDto dto)
    {
        var userId = GetEffectiveOwnerId();
        var apiary = await _context.Apiaries.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (apiary == null)
        {
            return NotFound("Pčelinjak nije pronađen.");
        }

        apiary.Name = dto.Name;
        apiary.Latitude = dto.Latitude;
        apiary.Longitude = dto.Longitude;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Beekeeper")]
    public async Task<IActionResult> DeleteApiary(int id)
    {
        var userId = GetEffectiveOwnerId();
        var apiary = await _context.Apiaries.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (apiary == null)
        {
            return NotFound("Pčelinjak nije pronađen.");
        }

        _context.Apiaries.Remove(apiary);
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
