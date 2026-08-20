namespace ERP.API.Domain.Entities;

public class Warehouse
{
    public Guid WarehouseID { get; set; } = Guid.NewGuid();
    public Guid CompanyID { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}

public class StockTransaction
{
    public Guid StockTransactionID { get; set; } = Guid.NewGuid();

    public Guid ProductID { get; set; }
    public Product? Product { get; set; }

    public Guid WarehouseID { get; set; }
    public Warehouse? Warehouse { get; set; }

    // Purchase, Sale, SalesReturn, PurchaseReturn, Transfer, Adjustment, Damage, OpeningBalance
    public string MovementType { get; set; } = string.Empty;

    // موجب = دخول للمخزون، سالب = خروج
    public decimal Quantity { get; set; }

    public string? ReferenceType { get; set; }
    public Guid? ReferenceID { get; set; }

    public Guid? UserID { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Snapshot مخزّن للأداء، يُحدَّث دائمًا من StockTransaction — ليس مصدر حقيقة مستقل.
/// </summary>
public class StockBalance
{
    public Guid StockBalanceID { get; set; } = Guid.NewGuid();
    public Guid ProductID { get; set; }
    public Guid WarehouseID { get; set; }
    public decimal CurrentQuantity { get; set; }
}
