namespace ERP.API.Domain.Entities;

public class AuditLog
{
    public Guid AuditID { get; set; } = Guid.NewGuid();
    public Guid? UserID { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityID { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;      // Create, Update, Delete
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? DeviceInfo { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
