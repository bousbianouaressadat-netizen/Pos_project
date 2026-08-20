namespace ERP.API.Application.DTOs;

// --- POS / Sale Invoice ---

public record SaleLineInputDto(Guid ProductID, decimal Qty, decimal UnitPrice, decimal DiscountAmount);

public record CreateSaleInvoiceDto(
    Guid? CustomerID, Guid WarehouseID,
    decimal InvoiceDiscountAmount, decimal PaidAmount, string PaymentMethod,
    string Status, // Held أو Completed
    List<SaleLineInputDto> Lines
);

public record SaleInvoiceLineDto(Guid ProductID, string ProductNameAR, decimal Qty, decimal UnitPrice, decimal DiscountAmount, decimal TaxAmount);

public record SaleInvoiceDto(
    Guid SaleInvoiceID, Guid? CustomerID, string? CustomerName,
    decimal TotalAmount, decimal DiscountAmount, decimal TaxAmount, decimal PaidAmount,
    string Status, string PaymentMethod, DateTime CreatedAt,
    List<SaleInvoiceLineDto> Lines
);

public record SaleInvoiceListItemDto(
    Guid SaleInvoiceID, string? CustomerName, decimal TotalAmount, decimal PaidAmount,
    string Status, DateTime CreatedAt
);

// --- Returns ---

public record CreateSaleReturnLineDto(Guid ProductID, decimal Qty, string? Reason);
public record CreateSaleReturnDto(List<CreateSaleReturnLineDto> Lines);

// --- Documents chain: Quotation → SalesOrder → DeliveryNote ---

public record CreateQuotationLineDto(Guid ProductID, decimal Qty, decimal UnitPrice);
public record CreateQuotationDto(Guid? CustomerID, List<CreateQuotationLineDto> Lines);
public record QuotationDto(Guid QuotationID, Guid? CustomerID, string Status, DateTime CreatedAt);

public record CreateSalesOrderFromQuotationDto(Guid QuotationID);
public record SalesOrderDto(Guid SalesOrderID, Guid? CustomerID, string Status, DateTime CreatedAt);

public record CreateDeliveryNoteFromOrderDto(Guid SalesOrderID);
public record DeliveryNoteDto(Guid DeliveryNoteID, Guid? ReferenceSalesOrderID, DateTime DeliveredAt);
