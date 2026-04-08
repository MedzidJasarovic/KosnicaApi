using System.ComponentModel.DataAnnotations;

namespace KosnicaApi.Models;

public class Shipment
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    public User? User { get; set; }

    public DateTime Date { get; set; }

    [Required]
    [MaxLength(100)]
    public string ReceiverName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ICollection<ShipmentItem> Items { get; set; } = new List<ShipmentItem>();
}
