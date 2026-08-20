namespace ERP.API.Application.DTOs;

public record UserListItemDto(
    Guid UserID, string Username, string FullName, bool IsActive,
    List<string> Roles, DateTime? LastLoginAt
);

public record CreateUserDto(string Username, string Password, string FullName, List<Guid> RoleIDs);

public record UpdateUserDto(string FullName, bool IsActive, List<Guid> RoleIDs);

public record ChangePasswordDto(string CurrentPassword, string NewPassword);
