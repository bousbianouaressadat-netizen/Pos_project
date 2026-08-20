using ERP.API.Application.DTOs;

namespace ERP.API.Application.Interfaces;

public interface IUserService
{
    Task<List<UserListItemDto>> GetAllAsync(Guid companyId);
    Task<UserListItemDto?> GetByIdAsync(Guid companyId, Guid userId);
    Task<Guid> CreateAsync(Guid companyId, CreateUserDto dto, Guid actingUserId);
    Task<bool> UpdateAsync(Guid companyId, Guid userId, UpdateUserDto dto, Guid actingUserId);
    Task<bool> DeactivateAsync(Guid companyId, Guid userId, Guid actingUserId);
    Task<bool> ChangePasswordAsync(Guid companyId, Guid userId, ChangePasswordDto dto);
}
