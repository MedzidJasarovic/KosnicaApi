using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KosnicaApi.Data;
using KosnicaApi.Models;
using KosnicaApi.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

namespace KosnicaApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
        {
            return BadRequest("Korisnik sa ovom email adresom već postoji.");
        }

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = UserRole.Beekeeper,
            Language = "sr" // default language as requested
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role
        });
    }

    [HttpPost("register-team")]
    [Authorize] // Only logged in users can add team members
    public async Task<ActionResult<AuthResponseDto>> RegisterTeamMember(RegisterTeamMemberDto dto)
    {
        var employerIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (employerIdString == null) return Unauthorized();

        var employerRole = User.FindFirstValue(ClaimTypes.Role);
        if (employerRole != UserRole.Beekeeper.ToString())
        {
            return Forbid(); // Only Beekeepers can hire!
        }

        if (dto.Role != UserRole.Assistant && dto.Role != UserRole.Veterinarian)
        {
            return BadRequest("Zaposlenom možete dodeliti samo ulogu Asistent (Assistant) ili Veterinar (Veterinarian).");
        }

        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
        {
            return BadRequest("Korisnik sa ovom email adresom već postoji.");
        }

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role,
            EmployerId = int.Parse(employerIdString),
            Language = "sr"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new AuthResponseDto
        {
            Token = "",
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            return Unauthorized("Pogrešan email ili lozinka.");
        }

        var token = GenerateJwtToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role
        });
    }

    [HttpGet("colleagues")]
    [Authorize(Roles = "Veterinarian")]
    public async Task<ActionResult<IEnumerable<VetColleagueDto>>> GetColleagues()
    {
        var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserIdString == null) return Unauthorized();
        var currentUserId = int.Parse(currentUserIdString);

        var currentUser = await _context.Users.FindAsync(currentUserId);
        if (currentUser == null) return NotFound("Korisnik nije pronađen.");

        // Find all veterinarians with the same first and last name
        var colleagues = await _context.Users
            .Include(u => u.Employer)
            .Where(u => u.Role == UserRole.Veterinarian && 
                        u.FirstName == currentUser.FirstName && 
                        u.LastName == currentUser.LastName)
            .Select(u => new VetColleagueDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                IsCurrentUser = u.Id == currentUserId,
                EmployerFirstName = u.Employer != null ? u.Employer.FirstName : null,
                EmployerLastName = u.Employer != null ? u.Employer.LastName : null,
                EmployerEmail = u.Employer != null ? u.Employer.Email : null
            })
            .ToListAsync();

        return Ok(colleagues);
    }

    private string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
        };

        if (user.EmployerId.HasValue)
        {
            claims.Add(new Claim("EmployerId", user.EmployerId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
