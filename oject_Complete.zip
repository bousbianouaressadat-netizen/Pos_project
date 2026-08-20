using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[Route("api/printing")]
public class PrintingController : ApiControllerBase
{
    private readonly IPrintingService _service;
    public PrintingController(IPrintingService service) => _service = service;

    [HttpPost("labels")]
    public async Task<ActionResult<List<BarcodeLabelDto>>> GetLabels([FromBody] List<Guid> productIds)
        => Ok(await _service.GetLabelsAsync(CompanyId, productIds));

    [HttpGet("invoice/{id:guid}")]
    public async Task<ActionResult<InvoicePrintDataDto>> GetInvoicePrintData(Guid id)
    {
        var data = await _service.GetInvoicePrintDataAsync(CompanyId, id);
        return data is null ? NotFound() : Ok(data);
    }
}
