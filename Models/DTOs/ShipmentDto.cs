using System.ComponentModel.DataAnnotations;

namespace KosnicaApi.Models.DTOs;

public class CreateShipmentDto
{
    [Required]
    [MaxLength(100)]
    public string ReceiverName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;

    public List<CreateShipmentItemDto> Items { get; set; } = new List<CreateShipmentItemDto>();
}

public class CreateShipmentItemDto
{
    [Required]
    public ProductType ProductType { get; set; }

    [Range(0.01, 1000000, ErrorMessage = "Količina mora biti veća od nule.")]
    public decimal Quantity { get; set; }
}

public class ShipmentDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<ShipmentItemDto> Items { get; set; } = new List<ShipmentItemDto>();
}

public class ShipmentItemDto
{
    public int Id { get; set; }
    public ProductType ProductType { get; set; }
    public decimal Quantity { get; set; }
}
