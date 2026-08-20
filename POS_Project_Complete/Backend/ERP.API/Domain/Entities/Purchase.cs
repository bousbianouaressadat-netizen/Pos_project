namespace ERP.API.Domain.Entities;

public class PurchaseOrder
{
    public Guid PurchaseOrderID { get; set; } = Guid.NewGuid();
    public Guid CompanyID { get; set; }
    public Guid SupplierID { get; set; }
    public Supplier? Supplier { get; set; }

    public string Status { get; set; } = "Open"; // Open, PartiallyReceived, Received, Cancelled
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PurchaseOrderLine> Lines { get; set; } = new List<PurchaseOrderLine>();
}

public class PurchaseOrderLine
{
    public Guid PurchaseOrderLineID { get; set; } = Guid.NewGuid();
    public Guid PurchaseOrderID { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }

    public Guid ProductID { get; set; }
    public decimal OrderedQty { get; set; }
    public decimal ReceivedQty { get; set; } // مُحدَّثة تلقائيًا من GoodsReceipt
    public decimal UnitPrice { get; set; }
}

public class GoodsReceipt
{
    public Guid GoodsReceiptID { get; set; } = Guid.NewGuid();
    public Guid CompanyID { get; set; }
    public Guid PurchaseOrderID { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }

    public Guid WarehouseID { get; set; }
    public Guid? UserID { get; set; }
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

    public ICollection<GoodsReceiptLine> Lines { get; set; } = new List<GoodsReceiptLine>();
}

public class GoodsReceiptLine
{
    public Guid GoodsReceiptLineID { get; set; } = Guid.NewGuid();
    public Guid GoodsReceiptID { get; set; }
    public GoodsReceipt? GoodsReceipt { get; set; }

    public Guid ProductID { get; set; }
    public decimal ReceivedQty { get; set; }
}

public class PurchaseInvoice
{
    public Guid PurchaseInvoiceID { get; set; } = Guid.NewGuid();
    public Guid CompanyID { get; set; }
    public Guid SupplierID { get; set; }
    public Guid? GoodsReceiptID { get; set; }

    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string Status { get; set; } = "Unpaid"; // Unpaid, PartiallyPaid, Paid
    public Guid? UserID { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PurchaseInvoiceLine> Lines { get; set; } = new List<PurchaseInvoiceLine>();
}

public class PurchaseInvoiceLine
{
    public Guid PurchaseInvoiceLineID { get; set; } = Guid.NewGuid();
    public Guid PurchaseInvoiceID { get; set; }
    public PurchaseInvoice? PurchaseInvoice { get; set; }

    public Guid ProductID { get; set; }
    public decimal Qty { get; set; }
    public decimal UnitPrice { get; set; }
}
