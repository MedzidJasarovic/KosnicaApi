using System.ComponentModel.DataAnnotations;

namespace KosnicaApi.Models;

public class User
{
    public int Id { get; set; }
    
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
    public required string PasswordHash { get; set; }
    
    public UserRole Role { get; set; } = UserRole.Beekeeper;

    public string Language { get; set; } = "sr"; // "sr" or "en"
    
    // For Assistants / vere
    public int? EmployerId { get; set; }
    public User? Employer { get; set; }
    
    // Navigation property
    public ICollection<Apiary> Apiaries { get; set; } = new List<Apiary>();
}
