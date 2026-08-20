namespace ERP.API.Domain.Entities;

public class Company
{
    public Guid CompanyID { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? NIF { get; set; }
    public string? NIS { get; set; }
    public string? RC { get; set; }
    public string? ArticleImposition { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string DefaultCurrency { get; set; } = "DZD";
    public string ActivityType { get; set; } = "General";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Role> Roles { get; set; } = new List<Role>();
}
