using System.Security.Claims;
using KosnicaApi.Data;
using KosnicaApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KosnicaApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class StatisticsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StatisticsController(ApplicationDbContext context)
    {
        _context = context;
    }

    private int GetEffectiveOwnerId()
    {
        var employerIdClaim = User.FindFirstValue("EmployerId");
        if (!string.IsNullOrEmpty(employerIdClaim)) return int.Parse(employerIdClaim);

        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<object>> GetDashboardStatistics()
    {
        var ownerId = GetEffectiveOwnerId();

        var apiariesCount = await _context.Apiaries.CountAsync(a => a.UserId == ownerId);
        
        var hivesCount = await _context.Hives
            .Include(h => h.Apiary)
            .CountAsync(h => h.Apiary!.UserId == ownerId);

        var plannedInterventionsCount = await _context.Interventions
            .Include(i => i.Apiary)
            .Include(i => i.Hive).ThenInclude(h => h!.Apiary)
            .CountAsync(i => i.Status == InterventionStatus.Planned && 
                             ((i.ApiaryId.HasValue && i.Apiary!.UserId == ownerId) || 
                              (i.HiveId.HasValue && i.Hive!.Apiary!.UserId == ownerId)));

        var recentTreatments = await _context.Treatments
            .Include(t => t.Hive).ThenInclude(h => h!.Apiary)
            .Where(t => t.Hive!.Apiary!.UserId == ownerId)
            .OrderByDescending(t => t.DateApplied)
            .Take(5)
            .Select(t => new 
            {
                t.Id,
                t.DateApplied,
                t.SubstanceName,
                HiveIdentifier = t.Hive!.Identifier
            })
            .ToListAsync();

        return Ok(new
        {
            TotalApiaries = apiariesCount,
            TotalHives = hivesCount,
            UpcomingInterventions = plannedInterventionsCount,
            RecentTreatments = recentTreatments
        });
    }
}
