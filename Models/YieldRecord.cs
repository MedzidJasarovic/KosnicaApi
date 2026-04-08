using System.ComponentModel.DataAnnotations;

namespace KosnicaApi.Models;

public class YieldRecord
{
    public int Id { get; set; }
    
    public int? HiveId { get; set; }
    public Hive? Hive { get; set; }

    public int? ApiaryId { get; set; }
    public Apiary? Apiary { get; set; }

    public DateTime Date { get; set; }

    public ProductType ProductType { get; set; }

    public decimal Quantity { get; set; }

    public string? Notes { get; set; }
    
    public string? WeatherConditions { get; set; }
}
