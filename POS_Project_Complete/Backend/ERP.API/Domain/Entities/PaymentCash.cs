namespace ERP.API.Domain.Entities;

public class Payment
{
    public Guid PaymentID { get; set; } = Guid.NewGuid();
    public Guid CompanyID { get; set; }

    public string Direction { get; set; } = string.Empty; // In, Out
    public string Method { get; set; } = string.Empty;    // Cash, Card, CCP, Other

    public decimal Amount { get; set; }

    public string? ReferenceType { get; set; } // SaleInvoice, CustomerDebt, PurchaseInvoice, SupplierDebt
    public Guid? ReferenceID { get; set; }

    public Guid? CustomerID { get; set; }
    public Guid? SupplierID { get; set; }

    public Guid UserID { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class CashSession
{
    public Guid CashSessionID { get; set; } = Guid.NewGuid();
    public Guid CompanyID { get; set; }
    public Guid UserID { get; set; }

    public decimal OpeningBalance { get; set; }
    public decimal? ExpectedCash { get; set; }  // محسوب تلقائيًا عند الإغلاق
    public decimal? ActualCash { get; set; }    // معدود يدويًا من المستخدم
    public decimal? Difference { get; set; }

    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }
    public string Status { get; set; } = "Open"; // Open, Closed
}

public class Expense
{
    public Guid ExpenseID { get; set; } = Guid.NewGuid();
    public Guid CompanyID { get; set; }

    public string Category { get; set; } = string.Empty; // Rent, Electricity, Water, Transport, Phone, Maintenance, Salary, Other
    public decimal Amount { get; set; }
    public string? Description { get; set; }

    public Guid UserID { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
