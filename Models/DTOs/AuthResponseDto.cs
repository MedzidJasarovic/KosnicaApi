namespace KosnicaApi.Models.DTOs;

public class AuthResponseDto
{
    public required string Token { get; set; }
    public int UserId { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public UserRole Role { get; set; }
}
