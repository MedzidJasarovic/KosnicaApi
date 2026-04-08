using System.ComponentModel.DataAnnotations;

namespace KosnicaApi.Models;

public class Hive
{
    public int Id { get; set; }
    
    public int ApiaryId { get; set; }
    public Apiary? Apiary { get; set; }

    [Required]
    [MaxLength(50)]
    public required string Identifier { get; set; }
    
    public HiveType Type { get; set; }
    
    // Raster map coordinates within the apiary
    public double? PositionX { get; set; }
    public double? PositionY { get; set; }

    // State Info
    [Range(1, 3)]
    public int NumberOfSupers { get; set; } // Nastavci
    public string? QueenStatus { get; set; } // Matica (e.g. Mlada, Stara, 2023)
    public string? ColonyStrength { get; set; } // Snaga drustva (Jako, Srednje, Slabo)

    // Navigation properties
    public ICollection<Intervention> Interventions { get; set; } = new List<Intervention>();
    public ICollection<YieldRecord> Yields { get; set; } = new List<YieldRecord>();
}
