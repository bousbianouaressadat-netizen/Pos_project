using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request.Username, request.Password);
        if (result is null)
            return Unauthorized(new { message = "اسم المستخدم أو كلمة السر غير صحيحة" });

        return Ok(result);
    }
}
