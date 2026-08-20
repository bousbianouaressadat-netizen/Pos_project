namespace ERP.API.Application.DTOs;

public record PermissionDto(Guid PermissionID, string Code, string DescriptionAR, string DescriptionFR, string Category);

public record RoleListItemDto(Guid RoleID, string Name, bool IsSystemRole, int UserCount);

public record RoleDetailDto(Guid RoleID, string Name, bool IsSystemRole, List<PermissionDto> Permissions);

public record CreateRoleDto(string Name, List<Guid> PermissionIDs);

public record UpdateRoleDto(string Name, List<Guid> PermissionIDs);
