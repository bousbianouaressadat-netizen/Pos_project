namespace ERP.API.Domain.Entities;

public class Customer
{
    public Guid CustomerID { get; set; } = Guid.NewGuid();
    public Guid CompanyID { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public Guid? PriceListID { get; set; }

    public decimal OpeningBalance { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CustomerTransaction> Transactions { get; set; } = new List<CustomerTransaction>();
}

public class CustomerTransaction
{
    public Guid CustomerTransactionID { get; set; } = Guid.NewGuid();
    public Guid CustomerID { get; set; }
    public Customer? Customer { get; set; }

    // موجب = دين على العميل (فاتورة)، سالب = تسديد لصالح العميل (دفعة/مرتجع/خصم)
    public string Type { get; set; } = string.Empty; // Invoice, Payment, Return, Discount
    public decimal Amount { get; set; }

    public string? ReferenceType { get; set; }
    public Guid? ReferenceID { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
