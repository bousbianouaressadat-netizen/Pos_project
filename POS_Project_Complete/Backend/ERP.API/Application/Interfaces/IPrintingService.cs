using ERP.API.Application.DTOs;

namespace ERP.API.Application.Interfaces;

public interface IPrintingService
{
    Task<List<BarcodeLabelDto>> GetLabelsAsync(Guid companyId, List<Guid> productIds);
    Task<InvoicePrintDataDto?> GetInvoicePrintDataAsync(Guid companyId, Guid invoiceId);
}
