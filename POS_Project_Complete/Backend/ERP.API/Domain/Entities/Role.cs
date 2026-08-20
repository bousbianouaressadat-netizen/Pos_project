namespace ERP.API.Domain.Entities;

public class Role
{
    public Guid RoleID { get; set; } = Guid.NewGuid();
    public Guid CompanyID { get; set; }
    public Company? Company { get; set; }

    public string Name { get; set; } = string.Empty; // Administrator, Manager, Cashier, StockManager, SalesManager, Custom...
    public bool IsSystemRole { get; set; } = false;   // الأدوار الأساسية لا تُحذف

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
