using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using ERP.API.Domain.Entities;
using ERP.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Application.Services;

public class RoleService : IRoleService
{
    private readonly AppDbContext _db;

    public RoleService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<RoleListItemDto>> GetAllAsync(Guid companyId)
    {
        var roles = await _db.Roles
            .Where(r => r.CompanyID == companyId)
            .Include(r => r.UserRoles)
            .ToListAsync();

        return roles.Select(r => new RoleListItemDto(r.RoleID, r.Name, r.IsSystemRole, r.UserRoles.Count)).ToList();
    }

    public async Task<RoleDetailDto?> GetByIdAsync(Guid companyId, Guid roleId)
    {
        var role = await _db.Roles
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.CompanyID == companyId && r.RoleID == roleId);

        if (role is null) return null;

        var perms = role.RolePermissions.Select(rp => new PermissionDto(
            rp.Permission!.PermissionID, rp.Permission.Code,
            rp.Permission.DescriptionAR, rp.Permission.DescriptionFR, rp.Permission.Category
        )).ToList();

        return new RoleDetailDto(role.RoleID, role.Name, role.IsSystemRole, perms);
    }

    public async Task<Guid> CreateAsync(Guid companyId, CreateRoleDto dto)
    {
        var role = new Role { CompanyID = companyId, Name = dto.Name, IsSystemRole = false };

        foreach (var permId in dto.PermissionIDs)
            role.RolePermissions.Add(new RolePermission { PermissionID = permId, Role = role });

        _db.Roles.Add(role);
        await _db.SaveChangesAsync();
        return role.RoleID;
    }

    public async Task<bool> UpdateAsync(Guid companyId, Guid roleId, UpdateRoleDto dto)
    {
        var role = await _db.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.CompanyID == companyId && r.RoleID == roleId);

        if (role is null) return false;
        if (role.IsSystemRole) throw new InvalidOperationException("لا يمكن تعديل الأدوار الأساسية للنظام");

        role.Name = dto.Name;
        _db.RolePermissions.RemoveRange(role.RolePermissions);
        foreach (var permId in dto.PermissionIDs)
            _db.RolePermissions.Add(new RolePermission { RoleID = roleId, PermissionID = permId });

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid companyId, Guid roleId)
    {
        var role = await _db.Roles
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.CompanyID == companyId && r.RoleID == roleId);

        if (role is null) return false;
        if (role.IsSystemRole) throw new InvalidOperationException("لا يمكن حذف الأدوار الأساسية للنظام");
        if (role.UserRoles.Any()) throw new InvalidOperationException("لا يمكن حذف دور مرتبط بمستخدمين حاليين");

        _db.Roles.Remove(role);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<PermissionDto>> GetAllPermissionsAsync()
    {
        return await _db.Permissions
            .Select(p => new PermissionDto(p.PermissionID, p.Code, p.DescriptionAR, p.DescriptionFR, p.Category))
            .ToListAsync();
    }
}
