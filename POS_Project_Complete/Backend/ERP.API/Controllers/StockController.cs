using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using ERP.API.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[Route("api/stock")]
public class StockController : ApiControllerBase
{
    private readonly IStockService _service;
    public StockController(IStockService service) => _service = service;

    [HttpGet("balance")]
    public async Task<ActionResult<List<StockBalanceDto>>> GetBalance([FromQuery] Guid? warehouseId)
        => Ok(await _service.GetBalanceAsync(CompanyId, warehouseId));

    [HttpGet("transactions/{productId:guid}")]
    public async Task<ActionResult<List<StockTransactionDto>>> GetTransactions(Guid productId)
        => Ok(await _service.GetTransactionsAsync(CompanyId, productId));

    [HttpPost("adjustment")]
    [RequirePermission("CanModifyStock")]
    public async Task<ActionResult> CreateAdjustment([FromBody] CreateStockAdjustmentDto dto)
    {
        await _service.CreateAdjustmentAsync(CompanyId, dto, CurrentUserId);
        return NoContent();
    }

    [HttpPost("transfer")]
    [RequirePermission("CanModifyStock")]
    public async Task<ActionResult> CreateTransfer([FromBody] CreateStockTransferDto dto)
    {
        try
        {
            await _service.CreateTransferAsync(CompanyId, dto, CurrentUserId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
