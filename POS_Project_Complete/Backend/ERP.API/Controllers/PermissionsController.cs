using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using ERP.API.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[Route("api/permissions")]
public class PermissionsController : ApiControllerBase
{
    private readonly IRoleService _roleService;

    public PermissionsController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    [RequirePermission("CanManageUsers")]
    public async Task<ActionResult<List<PermissionDto>>> GetAll()
        => Ok(await _roleService.GetAllPermissionsAsync());
}
