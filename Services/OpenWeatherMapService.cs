using System.Text.Json;
using KosnicaApi.Models.DTOs;

namespace KosnicaApi.Services;

public class OpenWeatherMapService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenWeatherMapService> _logger;

    public OpenWeatherMapService(HttpClient httpClient, IConfiguration configuration, ILogger<OpenWeatherMapService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<WeatherDto?> GetCurrentWeatherAsync(double latitude, double longitude)
    {
        var apiKey = _configuration["WeatherAPI:OpenWeatherMapKey"];
        
        // Return realistic mock data if API key is not configured for easy local development
        if (string.IsNullOrEmpty(apiKey) || apiKey == "MOCK_KEY" || apiKey == "YOUR_API_KEY_HERE")
        {
            _logger.LogInformation("Using MOCK weather data because API key is not set.");
            return GenerateMockWeather();
        }

        try
        {
            var url = $"https://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&appid={apiKey}&units=metric&lang=hr";
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Weather API returned {response.StatusCode}. Falling back to mock data.");
                return GenerateMockWeather(fallback: true);
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonString);
            var root = doc.RootElement;

            double temp = root.GetProperty("main").GetProperty("temp").GetDouble();
            int humidity = root.GetProperty("main").GetProperty("humidity").GetInt32();
            double speedMS = root.GetProperty("wind").GetProperty("speed").GetDouble();
            double windKmH = speedMS * 3.6; // Convert m/s to km/h
            
            var weatherArray = root.GetProperty("weather");
            string description = weatherArray.GetArrayLength() > 0 
                ? weatherArray[0].GetProperty("description").GetString() ?? "Nepoznato"
                : "Nepoznato";

            // Smart Logic: Determine if it's safe to open hives
            // Rule of thumb: Temp >= 14°C, Wind < 20km/h, not raining
            bool isSafe = temp >= 14.0 && windKmH < 20.0 && !description.ToLower().Contains("kiša");

            return new WeatherDto
            {
                TemperatureCelsius = Math.Round(temp, 1),
                HumidityPercent = humidity,
                WindSpeedKmH = Math.Round(windKmH, 1),
                Description = char.ToUpper(description[0]) + description.Substring(1),
                IsSafeToOpen = isSafe
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching weather data. Falling back to mock data.");
            return GenerateMockWeather(fallback: true);
        }
    }

    private WeatherDto GenerateMockWeather(bool fallback = false)
    {
        var random = new Random();
        double temp = random.Next(10, 25) + random.NextDouble();
        double wind = random.Next(5, 25) + random.NextDouble();
        bool isSafe = temp >= 14.0 && wind < 20.0;
        string description = fallback ? "Privremen prikaz (API ključ se aktivira)" : "Delimično oblačno";
        
        return new WeatherDto
        {
            TemperatureCelsius = Math.Round(temp, 1),
            HumidityPercent = random.Next(40, 70),
            WindSpeedKmH = Math.Round(wind, 1),
            Description = description,
            IsSafeToOpen = isSafe
        };
    }
}
