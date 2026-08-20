using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[Route("api/customers")]
public class CustomersController : ApiControllerBase
{
    private readonly ICustomerService _service;
    public CustomersController(ICustomerService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<CustomerListItemDto>>> GetAll() => Ok(await _service.GetAllAsync(CompanyId));

    [HttpGet("{id:guid}/ledger")]
    public async Task<ActionResult<CustomerLedgerDto>> GetLedger(Guid id)
    {
        var ledger = await _service.GetLedgerAsync(CompanyId, id);
        return ledger is null ? NotFound() : Ok(ledger);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateCustomerDto dto)
    {
        var id = await _service.CreateAsync(CompanyId, dto);
        return CreatedAtAction(nameof(GetLedger), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateCustomerDto dto)
    {
        var ok = await _service.UpdateAsync(CompanyId, id, dto);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/payments")]
    public async Task<ActionResult> RecordPayment(Guid id, [FromBody] RecordCustomerPaymentDto dto)
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
