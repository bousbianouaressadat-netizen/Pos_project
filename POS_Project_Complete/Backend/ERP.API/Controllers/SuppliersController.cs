using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[Route("api/suppliers")]
public class SuppliersController : ApiControllerBase
{
    private readonly ISupplierService _service;
    public SuppliersController(ISupplierService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<SupplierListItemDto>>> GetAll() => Ok(await _service.GetAllAsync(CompanyId));

    [HttpGet("{id:guid}/ledger")]
    public async Task<ActionResult<SupplierLedgerDto>> GetLedger(Guid id)
    {
        var ledger = await _service.GetLedgerAsync(CompanyId, id);
        return ledger is null ? NotFound() : Ok(ledger);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateSupplierDto dto)
    {
        var id = await _service.CreateAsync(CompanyId, dto);
        return CreatedAtAction(nameof(GetLedger), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateSupplierDto dto)
    {
        var ok = await _service.UpdateAsync(CompanyId, id, dto);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/payments")]
    public async Task<ActionResult> RecordPayment(Guid id, [FromBody] RecordSupplierPaymentDto dto)
    {
        try
        {
            var ok = await _service.RecordPaymentAsync(CompanyId, id, dto, CurrentUserId);
            return ok ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
