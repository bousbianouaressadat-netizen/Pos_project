using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using ERP.API.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[Route("api/backup")]
public class BackupController : ApiControllerBase
{
    private readonly IBackupService _service;
    public BackupController(IBackupService service) => _service = service;

    [HttpGet("history")]
    [RequirePermission("CanManageUsers")]
    public async Task<ActionResult<List<BackupHistoryDto>>> GetHistory() => Ok(await _service.GetHistoryAsync(CompanyId));

    [HttpPost("create")]
    [RequirePermission("CanManageUsers")]
    public async Task<ActionResult<BackupResultDto>> Create()
    {
        try
        {
            var result = await _service.CreateBackupAsync(CompanyId, CurrentUserId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost("restore")]
    [RequirePermission("CanManageUsers")]
    public async Task<ActionResult> Restore([FromBody] RestoreRequestDto dto)
    {
        try
        {
            await _service.RestoreAsync(CompanyId, dto.FileName, CurrentUserId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
