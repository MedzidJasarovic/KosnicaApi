using System.ComponentModel.DataAnnotations;

namespace KosnicaApi.Models.DTOs;

public class CreateApiaryDto
{
    public required string Name { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    [Range(1, 10)]
    public int Area { get; set; } = 10;
}

public class ApiaryDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public int HiveCount { get; set; }
    public int Area { get; set; }
}
