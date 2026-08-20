using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[Route("api/units")]
public class UnitsController : ApiControllerBase
{
    private readonly IUnitService _service;
    public UnitsController(IUnitService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<UnitDto>>> GetAll() => Ok(await _service.GetAllAsync(CompanyId));

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateUnitDto dto)
    {
        var id = await _service.CreateAsync(CompanyId, dto);
        return CreatedAtAction(nameof(GetAll), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateUnitDto dto)
    {
        var ok = await _service.UpdateAsync(CompanyId, id, dto);
        return ok ? NoContent() : NotFound();
    }
}
