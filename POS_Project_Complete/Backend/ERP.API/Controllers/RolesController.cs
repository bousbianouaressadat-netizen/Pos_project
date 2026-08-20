using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using ERP.API.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[Route("api/roles")]
public class RolesController : ApiControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    [RequirePermission("CanManageUsers")]
    public async Task<ActionResult<List<RoleListItemDto>>> GetAll()
        => Ok(await _roleService.GetAllAsync(CompanyId));

    [HttpGet("{id:guid}")]
    [RequirePermission("CanManageUsers")]
    public async Task<ActionResult<RoleDetailDto>> GetById(Guid id)
    {
        var role = await _roleService.GetByIdAsync(CompanyId, id);
        return role is null ? NotFound() : Ok(role);
    }

    [HttpPost]
    [RequirePermission("CanManageUsers")]
    public async Task<ActionResult> Create([FromBody] CreateRoleDto dto)
    {
        var id = await _roleService.CreateAsync(CompanyId, dto);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("CanManageUsers")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateRoleDto dto)
    {
        try
        {
            var ok = await _roleService.UpdateAsync(CompanyId, id, dto);
            return ok ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("CanManageUsers")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            var ok = await _roleService.DeleteAsync(CompanyId, id);
            return ok ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
