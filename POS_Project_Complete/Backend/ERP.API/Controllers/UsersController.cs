using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using ERP.API.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[Route("api/users")]
public class UsersController : ApiControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [RequirePermission("CanManageUsers")]
    public async Task<ActionResult<List<UserListItemDto>>> GetAll()
        => Ok(await _userService.GetAllAsync(CompanyId));

    [HttpGet("{id:guid}")]
    [RequirePermission("CanManageUsers")]
    public async Task<ActionResult<UserListItemDto>> GetById(Guid id)
    {
        var user = await _userService.GetByIdAsync(CompanyId, id);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    [RequirePermission("CanManageUsers")]
    public async Task<ActionResult> Create([FromBody] CreateUserDto dto)
    {
        try
        {
            var id = await _userService.CreateAsync(CompanyId, dto, CurrentUserId);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("CanManageUsers")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
    {
        var ok = await _userService.UpdateAsync(CompanyId, id, dto, CurrentUserId);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/deactivate")]
    [RequirePermission("CanManageUsers")]
    public async Task<ActionResult> Deactivate(Guid id)
    {
        var ok = await _userService.DeactivateAsync(CompanyId, id, CurrentUserId);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/change-password")]
    public async Task<ActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordDto dto)
    {
        // المستخدم يقدر يغيّر كلمة سره الخاصة فقط، إلا لو عنده CanManageUsers
        if (id != CurrentUserId && !User.HasClaim("Permission", "CanManageUsers"))
            return Forbid();

        try
        {
            var ok = await _userService.ChangePasswordAsync(CompanyId, id, dto);
            return ok ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
