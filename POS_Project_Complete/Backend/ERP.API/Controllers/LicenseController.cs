using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using ERP.API.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[Route("api/license")]
public class LicenseController : ApiControllerBase
{
    private readonly ILicenseService _service;
    public LicenseController(ILicenseService service) => _service = service;

    [HttpPost("activate")]
    [RequirePermission("CanManageUsers")]
    public async Task<ActionResult> Activate([FromBody] ActivateLicenseDto dto)
    {
        try
        {
            await _service.ActivateAsync(CompanyId, dto);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("status")]
    public async Task<ActionResult<LicenseStatusDto>> GetStatus([FromQuery] string deviceFingerprint)
        => Ok(await _service.GetStatusAsync(CompanyId, deviceFingerprint));

    [HttpPost("revoke-device")]
    [RequirePermission("CanManageUsers")]
    public async Task<ActionResult> RevokeDevice([FromBody] RevokeDeviceDto dto)
    {
        try
        {
            await _service.RevokeDeviceAsync(CompanyId, dto);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
