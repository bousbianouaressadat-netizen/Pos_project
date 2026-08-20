using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using ERP.API.Domain.Entities;
using ERP.API.Infrastructure.Data;
using ERP.API.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Application.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly JwtTokenGenerator _tokenGenerator;

    public AuthService(AppDbContext db, JwtTokenGenerator tokenGenerator)
    {
        _db = db;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<LoginResponseDto?> LoginAsync(string username, string password)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r!.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

        if (user is null) return null;
        if (!PasswordHasher.Verify(password, user.PasswordHash)) return null;

        var roleNames = user.UserRoles.Select(ur => ur.Role!.Name).Distinct().ToList();
        var permissionCodes = user.UserRoles
            .SelectMany(ur => ur.Role!.RolePermissions)
            .Select(rp => rp.Permission!.Code)
            .Distinct()
            .ToList();

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var token = _tokenGenerator.GenerateToken(user, permissionCodes, roleNames);

        return new LoginResponseDto(token, user.UserID, user.Username, user.FullName, roleNames, permissionCodes);
    }
}
