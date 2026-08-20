namespace ERP.API.Domain.Entities;

public class Permission
{
    public Guid PermissionID { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;        // CanSell, CanDiscount, CanChangePrice...
    public string DescriptionAR { get; set; } = string.Empty;
    public string DescriptionFR { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;    // Sales, Stock, Cash, Users, Reports...

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class RolePermission
{
    public Guid RoleID { get; set; }
    public Role? Role { get; set; }

    public Guid PermissionID { get; set; }
    public Permission? Permission { get; set; }
}

public class UserRole
{
    public Guid UserID { get; set; }
    public User? User { get; set; }

    public Guid RoleID { get; set; }
    public Role? Role { get; set; }
}
