using KosnicaApi.Models.DTOs;

namespace KosnicaApi.Services;

public interface IWeatherService
{
    Task<WeatherDto?> GetCurrentWeatherAsync(double latitude, double longitude);
}
