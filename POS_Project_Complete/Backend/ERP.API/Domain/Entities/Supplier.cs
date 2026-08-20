namespace ERP.API.Domain.Entities;

public class Supplier
{
    public Guid SupplierID { get; set; } = Guid.NewGuid();
    public Guid CompanyID { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }

    public decimal OpeningBalance { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<SupplierTransaction> Transactions { get; set; } = new List<SupplierTransaction>();
}

public class SupplierTransaction
{
    public Guid SupplierTransactionID { get; set; } = Guid.NewGuid();
    public Guid SupplierID { get; set; }
    public Supplier? Supplier { get; set; }

    // موجب = دين علينا للمورد (فاتورة شراء)، سالب = تسديد له (دفعة) أو مرتجع لصالحنا
    public string Type { get; set; } = string.Empty; // PurchaseInvoice, Payment, Return
    public decimal Amount { get; set; }

    public string? ReferenceType { get; set; }
    public Guid? ReferenceID { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
