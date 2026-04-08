using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KosnicaApi.Models;

public class ShipmentItem
{
    public int Id { get; set; }

    public int ShipmentId { get; set; }
    public Shipment? Shipment { get; set; }

    public ProductType ProductType { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Quantity { get; set; }
}
