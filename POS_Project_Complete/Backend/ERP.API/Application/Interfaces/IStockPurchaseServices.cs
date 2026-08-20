using ERP.API.Application.DTOs;

namespace ERP.API.Application.Interfaces;

/// <summary>
/// خدمة مركزية للمخزون — كل موديول (مشتريات، مبيعات، تعديل، تحويل) يمر عبرها إجباريًا
/// عشان يبقى StockTransaction هو المصدر الوحيد للحقيقة (Business Rule رقم 1).
/// </summary>
public interface IStockService
{
    Task RecordMovementAsync(Guid productId, Guid warehouseId, decimal quantity, string movementType,
        string? referenceType, Guid? referenceId, Guid? userId);

    Task<List<StockBalanceDto>> GetBalanceAsync(Guid companyId, Guid? warehouseId);
    Task<List<StockTransactionDto>> GetTransactionsAsync(Guid companyId, Guid productId);

    Task CreateAdjustmentAsync(Guid companyId, CreateStockAdjustmentDto dto, Guid userId);
    Task CreateTransferAsync(Guid companyId, CreateStockTransferDto dto, Guid userId);
}

public interface IPurchaseService
{
    Task<Guid> CreatePurchaseOrderAsync(Guid companyId, CreatePurchaseOrderDto dto);
    Task<List<PurchaseOrderDto>> GetPurchaseOrdersAsync(Guid companyId);
    Task<PurchaseOrderDto?> GetPurchaseOrderByIdAsync(Guid companyId, Guid id);

    Task<Guid> CreateGoodsReceiptAsync(Guid companyId, CreateGoodsReceiptDto dto, Guid userId);

    Task<Guid> CreatePurchaseInvoiceAsync(Guid companyId, CreatePurchaseInvoiceDto dto, Guid userId);
    Task<List<PurchaseInvoiceDto>> GetPurchaseInvoicesAsync(Guid companyId);
}
