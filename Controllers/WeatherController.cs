using System.Security.Claims;
using KosnicaApi.Data;
using KosnicaApi.Models.DTOs;
using KosnicaApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KosnicaApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class WeatherController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IWeatherService _weatherService;

    public WeatherController(ApplicationDbContext context, IWeatherService weatherService)
    {
        _context = context;
        _weatherService = weatherService;
    }

    private int GetEffectiveOwnerId()
    {
        var employerIdClaim = User.FindFirstValue("EmployerId");
        if (!string.IsNullOrEmpty(employerIdClaim)) return int.Parse(employerIdClaim);

        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    [HttpGet("apiaries/{apiaryId}")]
    public async Task<ActionResult<WeatherDto>> GetWeatherForApiary(int apiaryId)
    {
        var ownerId = GetEffectiveOwnerId();

        var apiary = await _context.Apiaries
            .FirstOrDefaultAsync(a => a.Id == apiaryId && a.UserId == ownerId);

        if (apiary == null)
        {
            return NotFound("Pčelinjak nije pronađen ili nemate dozvolu.");
        }

        if (!apiary.Latitude.HasValue || !apiary.Longitude.HasValue)
        {
            return BadRequest("Ovaj pčelinjak nema unete koordinate.");
        }

        var weather = await _weatherService.GetCurrentWeatherAsync((double)apiary.Latitude.Value, (double)apiary.Longitude.Value);

        if (weather == null)
        {
            return StatusCode(503, "Spoljni servis za vremensku prognozu privremeno nije dostupan.");
        }

        return Ok(weather);
    }
}
