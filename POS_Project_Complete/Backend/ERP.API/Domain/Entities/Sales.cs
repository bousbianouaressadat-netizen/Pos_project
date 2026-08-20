namespace ERP.API.Domain.Entities;

public class Quotation
{
    public Guid QuotationID { get; set; } = Guid.NewGuid();
    public Guid CompanyID { get; set; }
    public Guid? CustomerID { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Confirmed, Cancelled
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<QuotationLine> Lines { get; set; } = new List<QuotationLine>();
}

public class QuotationLine
{
    public Guid QuotationLineID { get; set; } = Guid.NewGuid();
    public Guid QuotationID { get; set; }
    public Quotation? Quotation { get; set; }

    public Guid ProductID { get; set; }
    public decimal Qty { get; set; }
    public decimal UnitPrice { get; set; }
}

public class SalesOrder
{
    public Guid SalesOrderID { get; set; } = Guid.NewGuid();
    public Guid CompanyID { get; set; }
    public Guid? CustomerID { get; set; }
    public Guid? ReferenceQuotationID { get; set; }
    public string Status { get; set; } = "Open";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<SalesOrderLine> Lines { get; set; } = new List<SalesOrderLine>();
}

public class SalesOrderLine
{
    public Guid SalesOrderLineID { get; set; } = Guid.NewGuid();
    public Guid SalesOrderID { get; set; }
    public SalesOrder? SalesOrder { get; set; }

    public Guid ProductID { get; set; }
    public decimal Qty { get; set; }
    public decimal UnitPrice { get; set; }
}

public class DeliveryNote
{
    public Guid DeliveryNoteID { get; set; } = Guid.NewGuid();
    public Guid CompanyID { get; set; }
    public Guid? ReferenceSalesOrderID { get; set; }
    public DateTime DeliveredAt { get; set; } = DateTime.UtcNow;

    public ICollection<DeliveryNoteLine> Lines { get; set; } = new List<DeliveryNoteLine>();
}

public class DeliveryNoteLine
{
    public Guid DeliveryNoteLineID { get; set; } = Guid.NewGuid();
    public Guid DeliveryNoteID { get; set; }
    public DeliveryNote? DeliveryNote { get; set; }

    public Guid ProductID { get; set; }
    public decimal Qty { get; set; }
}

/// <summary>
/// فاتورة البيع — نقطة خصم المخزون فعليًا (حسب القرار المُتَّخذ بالمرحلة الأولى)
/// وهي أيضًا شاشة POS الأساسية.
/// </summary>
public class SaleInvoice
{
    public Guid SaleInvoiceID { get; set; } = Guid.NewGuid();
    public Guid CompanyID { get; set; }
    public Guid? CustomerID { get; set; }
    public Guid UserID { get; set; }
    public Guid WarehouseID { get; set; }

    public Guid? ReferenceQuotationID { get; set; }
    public Guid? ReferenceOrderID { get; set; }

    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal PaidAmount { get; set; }

    public string Status { get; set; } = "Completed"; // Draft, Held, Completed, Returned
    public string PaymentMethod { get; set; } = "Cash"; // Cash, Card, CCP, Other, Mixed

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<SaleInvoiceLine> Lines { get; set; } = new List<SaleInvoiceLine>();
}

public class SaleInvoiceLine
{
    public Guid SaleInvoiceLineID { get; set; } = Guid.NewGuid();
    public Guid SaleInvoiceID { get; set; }
    public SaleInvoice? SaleInvoice { get; set; }

    public Guid ProductID { get; set; }
    public decimal Qty { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
}

public class SaleReturn
{
    public Guid SaleReturnID { get; set; } = Guid.NewGuid();
    public Guid CompanyID { get; set; }
    public Guid SaleInvoiceID { get; set; }
    public Guid UserID { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<SaleReturnLine> Lines { get; set; } = new List<SaleReturnLine>();
}

public class SaleReturnLine
{
    public Guid SaleReturnLineID { get; set; } = Guid.NewGuid();
    public Guid SaleReturnID { get; set; }
    public SaleReturn? SaleReturn { get; set; }

    public Guid ProductID { get; set; }
    public decimal Qty { get; set; }
    public string? Reason { get; set; }
}
