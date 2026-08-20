using System.Text.Json;
using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using ERP.API.Domain.Entities;
using ERP.API.Infrastructure.Data;
using ERP.API.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Application.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<UserListItemDto>> GetAllAsync(Guid companyId)
    {
        var users = await _db.Users
            .Where(u => u.CompanyID == companyId)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .ToListAsync();

        return users.Select(u => new UserListItemDto(
            u.UserID, u.Username, u.FullName, u.IsActive,
            u.UserRoles.Select(ur => ur.Role!.Name).ToList(),
            u.LastLoginAt
        )).ToList();
    }

    public async Task<UserListItemDto?> GetByIdAsync(Guid companyId, Guid userId)
    {
        var u = await _db.Users
            .Include(x => x.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(x => x.CompanyID == companyId && x.UserID == userId);

        if (u is null) return null;

        return new UserListItemDto(u.UserID, u.Username, u.FullName, u.IsActive,
            u.UserRoles.Select(ur => ur.Role!.Name).ToList(), u.LastLoginAt);
    }

    public async Task<Guid> CreateAsync(Guid companyId, CreateUserDto dto, Guid actingUserId)
    {
        var exists = await _db.Users.AnyAsync(u => u.CompanyID == companyId && u.Username == dto.Username);
        if (exists) throw new InvalidOperationException("اسم المستخدم موجود مسبقًا");

        var user = new User
        {
            CompanyID = companyId,
            Username = dto.Username,
            PasswordHash = PasswordHasher.Hash(dto.Password),
            FullName = dto.FullName,
            IsActive = true
        };

        foreach (var roleId in dto.RoleIDs)
            user.UserRoles.Add(new UserRole { RoleID = roleId, User = user });

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        await LogAudit(actingUserId, nameof(User), user.UserID.ToString(), "Create", null, JsonSerializer.Serialize(new { user.Username, user.FullName }));

        return user.UserID;
    }

    public async Task<bool> UpdateAsync(Guid companyId, Guid userId, UpdateUserDto dto, Guid actingUserId)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.CompanyID == companyId && u.UserID == userId);

        if (user is null) return false;

        var oldValue = JsonSerializer.Serialize(new { user.FullName, user.IsActive });

        user.FullName = dto.FullName;
        user.IsActive = dto.IsActive;

        _db.UserRoles.RemoveRange(user.UserRoles);
        foreach (var roleId in dto.RoleIDs)
            _db.UserRoles.Add(new UserRole { UserID = userId, RoleID = roleId });

        await _db.SaveChangesAsync();

        await LogAudit(actingUserId, nameof(User), userId.ToString(), "Update", oldValue,
            JsonSerializer.Serialize(new { dto.FullName, dto.IsActive }));

        return true;
    }

    public async Task<bool> DeactivateAsync(Guid companyId, Guid userId, Guid actingUserId)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.CompanyID == companyId && u.UserID == userId);
        if (user is null) return false;

        user.IsActive = false;
        await _db.SaveChangesAsync();

        await LogAudit(actingUserId, nameof(User), userId.ToString(), "Deactivate", null, null);
        return true;
    }

    public async Task<bool> ChangePasswordAsync(Guid companyId, Guid userId, ChangePasswordDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.CompanyID == companyId && u.UserID == userId);
        if (user is null) return false;

        if (!PasswordHasher.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new InvalidOperationException("كلمة السر الحالية غير صحيحة");

        user.PasswordHash = PasswordHasher.Hash(dto.NewPassword);
        await _db.SaveChangesAsync();
        return true;
    }

    private async Task LogAudit(Guid userId, string entity, string entityId, string action, string? oldVal, string? newVal)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            UserID = userId,
            EntityName = entity,
            EntityID = entityId,
            Action = action,
            OldValue = oldVal,
            NewValue = newVal
        });
        await _db.SaveChangesAsync();
    }
}
