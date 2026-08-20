using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[Route("api/categories")]
public class CategoriesController : ApiControllerBase
{
    private readonly ICategoryService _service;
    public CategoriesController(ICategoryService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll() => Ok(await _service.GetAllAsync(CompanyId));

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        var id = await _service.CreateAsync(CompanyId, dto);
        return CreatedAtAction(nameof(GetAll), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateCategoryDto dto)
    {
        var ok = await _service.UpdateAsync(CompanyId, id, dto);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult> Deactivate(Guid id)
    {
        var ok = await _service.DeactivateAsync(CompanyId, id);
        return ok ? NoContent() : NotFound();
    }
}
