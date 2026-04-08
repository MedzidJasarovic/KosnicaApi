using System.Security.Claims;
using KosnicaApi.Data;
using KosnicaApi.Models;
using KosnicaApi.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KosnicaApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class StorageController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StorageController(ApplicationDbContext context)
    {
        _context = context;
    }

    private int GetEffectiveOwnerId()
    {
        var employerIdClaim = User.FindFirstValue("EmployerId");
        if (!string.IsNullOrEmpty(employerIdClaim)) return int.Parse(employerIdClaim);

        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<IEnumerable<YieldStatisticsDto>>> GetStatistics([FromQuery] string viewMode = "all")
    {
        var ownerId = GetEffectiveOwnerId();

        // Get total yields per product type
        var yieldsQuery = _context.YieldRecords
            .Include(y => y.Apiary)
            .Include(y => y.Hive).ThenInclude(h => h!.Apiary)
            .AsQueryable();

        yieldsQuery = yieldsQuery.Where(y => 
            (y.ApiaryId.HasValue && y.Apiary!.UserId == ownerId) || 
            (y.HiveId.HasValue && y.Hive!.Apiary!.UserId == ownerId));

        var productionStats = await yieldsQuery
            .GroupBy(y => y.ProductType)
            .Select(g => new { ProductType = g.Key, Total = g.Sum(y => y.Quantity) })
            .ToListAsync();

        if (viewMode == "all")
        {
            return Ok(productionStats.Select(s => new YieldStatisticsDto 
            { 
                ProductType = s.ProductType, 
                TotalQuantity = s.Total 
            }));
        }

        // Get total shipments per product type
        var shipmentStats = await _context.ShipmentItems
            .Include(si => si.Shipment)
            .Where(si => si.Shipment!.UserId == ownerId)
            .GroupBy(si => si.ProductType)
            .Select(g => new { ProductType = g.Key, Total = g.Sum(si => si.Quantity) })
            .ToListAsync();

        // Calculate current stock
        var allProductTypes = Enum.GetValues<ProductType>();
        var currentStock = allProductTypes.Select(type => {
            var prod = productionStats.FirstOrDefault(p => p.ProductType == type)?.Total ?? 0;
            var ship = shipmentStats.FirstOrDefault(s => s.ProductType == type)?.Total ?? 0;
            return new YieldStatisticsDto
            {
                ProductType = type,
                TotalQuantity = Math.Max(0, prod - ship)
            };
        }).Where(s => s.TotalQuantity > 0).ToList();

        return Ok(currentStock);
    }

    [HttpGet("shipments")]
    public async Task<ActionResult<IEnumerable<ShipmentDto>>> GetShipments()
    {
        var ownerId = GetEffectiveOwnerId();

        var shipments = await _context.Shipments
            .Include(s => s.Items)
            .Where(s => s.UserId == ownerId)
            .OrderByDescending(s => s.Date)
            .Select(s => new ShipmentDto
            {
                Id = s.Id,
                Date = s.Date,
                ReceiverName = s.ReceiverName,
                Description = s.Description,
                Items = s.Items.Select(i => new ShipmentItemDto
                {
                    Id = i.Id,
                    ProductType = i.ProductType,
                    Quantity = i.Quantity
                }).ToList()
            })
            .ToListAsync();

        return Ok(shipments);
    }

    [HttpPost("shipments")]
    public async Task<ActionResult<ShipmentDto>> CreateShipment([FromBody] CreateShipmentDto dto)
    {
        if (dto == null || dto.Items == null || !dto.Items.Any()) return BadRequest("Nema pošiljki za dodavanje.");

        var ownerId = GetEffectiveOwnerId();

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Get current stock for all requested product types to validate
            var requestedTypes = dto.Items.Select(i => i.ProductType).Distinct().ToList();
            
            foreach (var type in requestedTypes)
            {
                var totalRequested = dto.Items.Where(i => i.ProductType == type).Sum(i => i.Quantity);
                
                var prod = await _context.YieldRecords
                    .Where(y => ((y.ApiaryId.HasValue && y.Apiary!.UserId == ownerId) || (y.HiveId.HasValue && y.Hive!.Apiary!.UserId == ownerId)) && y.ProductType == type)
                    .SumAsync(y => y.Quantity);
                
                var ship = await _context.ShipmentItems
                    .Include(si => si.Shipment)
                    .Where(si => si.Shipment!.UserId == ownerId && si.ProductType == type)
                    .SumAsync(si => si.Quantity);

                if (prod - ship < totalRequested)
                {
                    return BadRequest("Nemate dovoljno proizvoda na stanju.");
                }
            }

            // 2. Create the unified shipment
            var shipment = new Shipment
            {
                UserId = ownerId,
                Date = dto.Date.ToUniversalTime(),
                ReceiverName = dto.ReceiverName,
                Description = dto.Description,
                Items = dto.Items.Select(i => new ShipmentItem 
                {
                    ProductType = i.ProductType,
                    Quantity = i.Quantity
                }).ToList()
            };

            _context.Shipments.Add(shipment);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var resultDto = new ShipmentDto
            {
                Id = shipment.Id,
                Date = shipment.Date,
                ReceiverName = shipment.ReceiverName,
                Description = shipment.Description,
                Items = shipment.Items.Select(i => new ShipmentItemDto
                {
                    Id = i.Id,
                    ProductType = i.ProductType,
                    Quantity = i.Quantity
                }).ToList()
            };

            return Ok(resultDto);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "Greška prilikom obrade pošiljke: " + ex.Message);
        }
    }
}
