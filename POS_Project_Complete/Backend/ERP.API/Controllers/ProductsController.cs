using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using ERP.API.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[Route("api/products")]
public class ProductsController : ApiControllerBase
{
    private readonly IProductService _service;
    public ProductsController(IProductService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<ProductListItemDto>>> GetAll([FromQuery] string? search)
        => Ok(await _service.GetAllAsync(CompanyId, search));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDetailDto>> GetById(Guid id)
    {
        var product = await _service.GetByIdAsync(CompanyId, id);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpGet("barcode/{code}")]
    public async Task<ActionResult<ProductByBarcodeDto>> GetByBarcode(string code)
    {
        var product = await _service.GetByBarcodeAsync(CompanyId, code);
        return product is null ? NotFound(new { message = "المنتج غير موجود بهذا الباركود" }) : Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateProductDto dto)
    {
        try
        {
            var id = await _service.CreateAsync(CompanyId, dto, CurrentUserId);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("CanChangePrice")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateProductDto dto)
    {
        var ok = await _service.UpdateAsync(CompanyId, id, dto, CurrentUserId);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult> Deactivate(Guid id)
    {
        var ok = await _service.DeactivateAsync(CompanyId, id, CurrentUserId);
        return ok ? NoContent() : NotFound();
    }
}
