using Microsoft.AspNetCore.Authorization;

namespace ERP.API.Infrastructure.Security;

/// <summary>
/// يُستخدم على الـ Controllers/Actions للتحقق من صلاحية محددة (Permission Code)
/// وليس فقط الاعتماد على الدور (Role) — حسب Business Rule رقم 4.
/// مثال: [RequirePermission("CanDeleteSale")]
/// </summary>
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permissionCode)
    {
        Policy = $"Permission:{permissionCode}";
    }
}

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.HasClaim(c => c.Type == "Permission" && c.Value == requirement.PermissionCode))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}

public class PermissionRequirement : IAuthorizationRequirement
{
    public string PermissionCode { get; }
    public PermissionRequirement(string permissionCode) => PermissionCode = permissionCode;
}
