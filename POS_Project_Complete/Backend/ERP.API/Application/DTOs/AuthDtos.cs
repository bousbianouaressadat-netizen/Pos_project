namespace ERP.API.Application.DTOs;

public record LoginRequestDto(string Username, string Password);

public record LoginResponseDto(
    string Token,
    Guid UserID,
    string Username,
    string FullName,
    List<string> Roles,
    List<string> Permissions
);
