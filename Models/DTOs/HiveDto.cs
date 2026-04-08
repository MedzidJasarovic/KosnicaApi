using System.ComponentModel.DataAnnotations;

namespace KosnicaApi.Models.DTOs;

public class CreateHiveDto
{
    public required string Identifier { get; set; }
    public HiveType Type { get; set; }
    public double? PositionX { get; set; }
    public double? PositionY { get; set; }
    public int NumberOfSupers { get; set; }
    public string? QueenStatus { get; set; }
    public string? ColonyStrength { get; set; }
}

public class HiveDto
{
    public int Id { get; set; }
    public required string Identifier { get; set; }
    public HiveType Type { get; set; }
    public double? PositionX { get; set; }
    public double? PositionY { get; set; }
    public int NumberOfSupers { get; set; }
    public string? QueenStatus { get; set; }
    public string? ColonyStrength { get; set; }
}
