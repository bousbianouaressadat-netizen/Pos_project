using ERP.API.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers;

public record WarehouseDto(Guid WarehouseID, string Name, bool IsDefault);

[Route("api/warehouses")]
public class WarehousesController : ApiControllerBase
{
    private readonly AppDbContext _db;
    public WarehousesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<WarehouseDto>>> GetAll()
    {
        var list = await _db.Warehouses
            .Where(w => w.CompanyID == CompanyId)
            .Select(w => new WarehouseDto(w.WarehouseID, w.Name, w.IsDefault))
            .ToListAsync();

        return Ok(list);
    }
}
