namespace ERP.API.Domain.Entities;

public class User
{
    public Guid UserID { get; set; } = Guid.NewGuid();
    public Guid CompanyID { get; set; }
    public Company? Company { get; set; }

    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
