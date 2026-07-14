using System.ComponentModel.DataAnnotations;

namespace KosnicaApi.Models.DTOs;

public class RegisterDto
{
    [Required(ErrorMessage = "Ime je obavezno.")]
    [MinLength(2, ErrorMessage = "Ime mora imati najmanje 2 karaktera.")]
    [MaxLength(50, ErrorMessage = "Ime ne može biti duže od 50 karaktera.")]
    public required string FirstName { get; set; }

    [Required(ErrorMessage = "Prezime je obavezno.")]
    [MinLength(2, ErrorMessage = "Prezime mora imati najmanje 2 karaktera.")]
    [MaxLength(50, ErrorMessage = "Prezime ne može biti duže od 50 karaktera.")]
    public required string LastName { get; set; }

    [Required(ErrorMessage = "Email adresa je obavezna.")]
    [EmailAddress(ErrorMessage = "Email adresa nije ispravnog formata.")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Lozinka je obavezna.")]
    [MinLength(6, ErrorMessage = "Lozinka mora imati najmanje 6 karaktera.")]
    public required string Password { get; set; }
}
