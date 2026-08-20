namespace ERP.API.Application.DTOs;

public record StockBalanceDto(Guid ProductID, string ProductNameAR, Guid WarehouseID, string WarehouseName, decimal CurrentQuantity);

public record StockTransactionDto(
    Guid StockTransactionID, Guid ProductID, string ProductNameAR,
    string MovementType, decimal Quantity, string? ReferenceType, DateTime Timestamp
);

public record StockAdjustmentLineDto(Guid ProductID, decimal Quantity, string Reason);
public record CreateStockAdjustmentDto(Guid WarehouseID, List<StockAdjustmentLineDto> Lines);

public record StockTransferLineDto(Guid ProductID, decimal Quantity);
public record CreateStockTransferDto(Guid FromWarehouseID, Guid ToWarehouseID, List<StockTransferLineDto> Lines);

// --- Purchases ---

public record PurchaseOrderLineDto(Guid ProductID, string ProductNameAR, decimal OrderedQty, decimal ReceivedQty, decimal UnitPrice);
public record PurchaseOrderDto(Guid PurchaseOrderID, Guid SupplierID, string SupplierName, string Status, DateTime CreatedAt, List<PurchaseOrderLineDto> Lines);

public record CreatePurchaseOrderLineDto(Guid ProductID, decimal OrderedQty, decimal UnitPrice);
public record CreatePurchaseOrderDto(Guid SupplierID, List<CreatePurchaseOrderLineDto> Lines);

public record CreateGoodsReceiptLineDto(Guid ProductID, decimal ReceivedQty);
public record CreateGoodsReceiptDto(Guid PurchaseOrderID, Guid WarehouseID, List<CreateGoodsReceiptLineDto> Lines);

public record CreatePurchaseInvoiceLineDto(Guid ProductID, decimal Qty, decimal UnitPrice);
public record CreatePurchaseInvoiceDto(Guid SupplierID, Guid? GoodsReceiptID, decimal PaidAmount, List<CreatePurchaseInvoiceLineDto> Lines);

public record PurchaseInvoiceDto(
    Guid PurchaseInvoiceID, Guid SupplierID, string SupplierName,
    decimal TotalAmount, decimal PaidAmount, string Status, DateTime CreatedAt
);
