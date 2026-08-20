using ERP.API.Application.DTOs;

namespace ERP.API.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(string username, string password);
}
