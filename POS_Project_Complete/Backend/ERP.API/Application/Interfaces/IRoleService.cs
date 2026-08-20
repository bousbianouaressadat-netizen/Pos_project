using ERP.API.Application.DTOs;

namespace ERP.API.Application.Interfaces;

public interface IRoleService
{
    Task<List<RoleListItemDto>> GetAllAsync(Guid companyId);
    Task<RoleDetailDto?> GetByIdAsync(Guid companyId, Guid roleId);
    Task<Guid> CreateAsync(Guid companyId, CreateRoleDto dto);
    Task<bool> UpdateAsync(Guid companyId, Guid roleId, UpdateRoleDto dto);
    Task<bool> DeleteAsync(Guid companyId, Guid roleId);
    Task<List<PermissionDto>> GetAllPermissionsAsync();
}
