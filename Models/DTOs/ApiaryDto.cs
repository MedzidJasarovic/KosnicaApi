namespace KosnicaApi.Models.DTOs;

public class CreateApiaryDto
{
    public required string Name { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}

public class ApiaryDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public int HiveCount { get; set; }
}
