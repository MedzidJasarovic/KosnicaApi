namespace KosnicaApi.Models.DTOs;

public class CreateYieldDto
{
    public int? HiveId { get; set; }
    public int? ApiaryId { get; set; }
    public DateTime Date { get; set; }
    public ProductType ProductType { get; set; }
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }
    public string? WeatherConditions { get; set; }
}

public class YieldDto
{
    public int Id { get; set; }
    public int? HiveId { get; set; }
    public int? ApiaryId { get; set; }
    public DateTime Date { get; set; }
    public ProductType ProductType { get; set; }
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }
    public string? WeatherConditions { get; set; }
    public string? ApiaryName { get; set; }
    public string? HiveIdentifier { get; set; }
}

public class YieldStatisticsDto
{
    public ProductType ProductType { get; set; }
    public decimal TotalQuantity { get; set; }
}
