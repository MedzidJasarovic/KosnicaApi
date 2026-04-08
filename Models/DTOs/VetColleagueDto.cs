namespace KosnicaApi.Models.DTOs;

public class VetColleagueDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsCurrentUser { get; set; }
    public string? EmployerFirstName { get; set; }
    public string? EmployerLastName { get; set; }
    public string? EmployerEmail { get; set; }
}
