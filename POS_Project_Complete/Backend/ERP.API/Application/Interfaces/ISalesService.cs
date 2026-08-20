using ERP.API.Application.DTOs;

namespace ERP.API.Application.Interfaces;

public interface ISalesService
{
    // POS
    Task<Guid> CreateSaleInvoiceAsync(Guid companyId, CreateSaleInvoiceDto dto, Guid userId);
    Task<List<SaleInvoiceListItemDto>> GetInvoicesAsync(Guid companyId);
    Task<SaleInvoiceDto?> GetInvoiceByIdAsync(Guid companyId, Guid invoiceId);
    Task<bool> DeleteInvoiceAsync(Guid companyId, Guid invoiceId, Guid userId); // يتطلب CanDeleteSale
    Task<Guid> CreateReturnAsync(Guid companyId, Guid invoiceId, CreateSaleReturnDto dto, Guid userId);

    // وثائق مترابطة
    Task<Guid> CreateQuotationAsync(Guid companyId, CreateQuotationDto dto);
    Task<Guid> CreateSalesOrderFromQuotationAsync(Guid companyId, CreateSalesOrderFromQuotationDto dto);
    Task<Guid> CreateDeliveryNoteFromOrderAsync(Guid companyId, CreateDeliveryNoteFromOrderDto dto);
}
