namespace ERP.API.Domain.Entities;

public class License
{
    public Guid LicenseID { get; set; } = Guid.NewGuid();
    public Guid CompanyID { get; set; }

    // Hash فقط، وليس المفتاح الأصلي أبدًا (Business Rule 8)
    public string LicenseKeyHash { get; set; } = string.Empty;

    public string DeviceFingerprint { get; set; } = string.Empty;
    public DateTime ActivatedAt { get; set; } = DateTime.UtcNow;
    public int MaxDevices { get; set; } = 1;

    public string Status { get; set; } = "Active"; // Active, Revoked, Transferred
}
