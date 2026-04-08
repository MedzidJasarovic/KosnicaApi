namespace KosnicaApi.Models.DTOs;

public class WeatherDto
{
    public required string Description { get; set; }
    public double TemperatureCelsius { get; set; }
    public double WindSpeedKmH { get; set; }
    public int HumidityPercent { get; set; }
    public bool IsSafeToOpen { get; set; }
}
