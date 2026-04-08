using System.ComponentModel.DataAnnotations;

namespace KosnicaApi.Models.DTOs;

public class RegisterTeamMemberDto
{
    [Required]
    [MaxLength(50)]
    public required string FirstName { get; set; }
    
    [Required]
    [MaxLength(50)]
    public required string LastName { get; set; }
    
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
    
    [Required]
    [MinLength(6)]
    public required string Password { get; set; }
    
    public UserRole Role { get; set; } // Should be Assistant or Veterinarian
}
