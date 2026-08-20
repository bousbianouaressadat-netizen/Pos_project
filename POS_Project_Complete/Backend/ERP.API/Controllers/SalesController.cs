using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using ERP.API.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[Route("api/sales/invoices")]
public class SaleInvoicesController : ApiControllerBase
{
    private readonly ISalesService _service;
    public SaleInvoicesController(ISalesService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<SaleInvoiceListItemDto>>> GetAll() => Ok(await _service.GetInvoicesAsync(CompanyId));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SaleInvoiceDto>> GetById(Guid id)
    {
        var invoice = await _service.GetInvoiceByIdAsync(CompanyId, id);
        return invoice is null ? NotFound() : Ok(invoice);
    }

    [HttpPost]
    [RequirePermission("CanSell")]
    public async Task<ActionResult> Create([FromBody] CreateSaleInvoiceDto dto)
    {
        try
        {
            var id = await _service.CreateSaleInvoiceAsync(CompanyId, dto, CurrentUserId);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("CanDeleteSale")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var ok = await _service.DeleteInvoiceAsync(CompanyId, id, CurrentUserId);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/return")]
    [RequirePermission("CanReturn")]
    public async Task<ActionResult> CreateReturn(Guid id, [FromBody] CreateSaleReturnDto dto)
    {
        try
        {
            var returnId = await _service.CreateReturnAsync(CompanyId, id, dto, CurrentUserId);
            return Ok(new { id = returnId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

[Route("api/sales/quotations")]
public class QuotationsController : ApiControllerBase
{
    private readonly ISalesService _service;
    public QuotationsController(ISalesService service) => _service = service;

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateQuotationDto dto)
    {
        var id = await _service.CreateQuotationAsync(CompanyId, dto);
        return Ok(new { id });
    }
}

[Route("api/sales/orders")]
public class SalesOrdersController : ApiControllerBase
{
    private readonly ISalesService _service;
    public SalesOrdersController(ISalesService service) => _service = service;

    [HttpPost("from-quotation")]
    public async Task<ActionResult> CreateFromQuotation([FromBody] CreateSalesOrderFromQuotationDto dto)
    {
        try
        {
            var id = await _service.CreateSalesOrderFromQuotationAsync(CompanyId, dto);
            return Ok(new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

[Route("api/sales/delivery-notes")]
public class DeliveryNotesController : ApiControllerBase
{
    private readonly ISalesService _service;
    public DeliveryNotesController(ISalesService service) => _service = service;

    [HttpPost("from-order")]
    public async Task<ActionResult> CreateFromOrder([FromBody] CreateDeliveryNoteFromOrderDto dto)
    {
        try
        {
            var id = await _service.CreateDeliveryNoteFromOrderAsync(CompanyId, dto);
            return Ok(new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
