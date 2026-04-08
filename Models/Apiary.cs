using System.ComponentModel.DataAnnotations;

namespace KosnicaApi.Models;

public class Apiary
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    public User? User { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }
    
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    // Navigation property
    public ICollection<Hive> Hives { get; set; } = new List<Hive>();
}
