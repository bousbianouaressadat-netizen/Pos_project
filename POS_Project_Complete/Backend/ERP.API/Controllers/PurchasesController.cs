using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[Route("api/purchase-orders")]
public class PurchaseOrdersController : ApiControllerBase
{
    private readonly IPurchaseService _service;
    public PurchaseOrdersController(IPurchaseService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<PurchaseOrderDto>>> GetAll() => Ok(await _service.GetPurchaseOrdersAsync(CompanyId));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PurchaseOrderDto>> GetById(Guid id)
    {
        var order = await _service.GetPurchaseOrderByIdAsync(CompanyId, id);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreatePurchaseOrderDto dto)
    {
        var id = await _service.CreatePurchaseOrderAsync(CompanyId, dto);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }
}

[Route("api/goods-receipts")]
public class GoodsReceiptsController : ApiControllerBase
{
    private readonly IPurchaseService _service;
    public GoodsReceiptsController(IPurchaseService service) => _service = service;

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateGoodsReceiptDto dto)
    {
        try
        {
            var id = await _service.CreateGoodsReceiptAsync(CompanyId, dto, CurrentUserId);
            return Ok(new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

[Route("api/purchase-invoices")]
public class PurchaseInvoicesController : ApiControllerBase
{
    private readonly IPurchaseService _service;
    public PurchaseInvoicesController(IPurchaseService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<PurchaseInvoiceDto>>> GetAll() => Ok(await _service.GetPurchaseInvoicesAsync(CompanyId));

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreatePurchaseInvoiceDto dto)
    {
        var id = await _service.CreatePurchaseInvoiceAsync(CompanyId, dto, CurrentUserId);
        return Ok(new { id });
    }
}
