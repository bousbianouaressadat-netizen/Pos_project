namespace ERP.API.Application.DTOs;

// الباركود نفسه (الشكل EAN13/Code128) يُرسَم في الواجهة (JsBarcode/ZXing.js)،
// الـ API يوفر فقط البيانات اللازمة للملصق أو الفاتورة.

public record BarcodeLabelDto(string SKU, string NameAR, string NameFR, string BarcodeCode, decimal Price);

public record InvoicePrintDataDto(
    Guid SaleInvoiceID, string CompanyName, string? CompanyNIF, string? CompanyRC,
    string? CustomerName, DateTime CreatedAt,
    List<InvoicePrintLineDto> Lines,
    decimal TotalAmount, decimal DiscountAmount, decimal TaxAmount, decimal PaidAmount, decimal RemainingAmount
);

public record InvoicePrintLineDto(string NameAR, decimal Qty, decimal UnitPrice, decimal LineTotal);
