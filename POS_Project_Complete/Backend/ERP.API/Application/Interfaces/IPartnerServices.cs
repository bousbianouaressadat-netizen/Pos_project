using ERP.API.Application.DTOs;

namespace ERP.API.Application.Interfaces;

public interface ICustomerService
{
    Task<List<CustomerListItemDto>> GetAllAsync(Guid companyId);
    Task<CustomerLedgerDto?> GetLedgerAsync(Guid companyId, Guid customerId);
    Task<Guid> CreateAsync(Guid companyId, CreateCustomerDto dto);
    Task<bool> UpdateAsync(Guid companyId, Guid id, UpdateCustomerDto dto);
    Task<bool> RecordPaymentAsync(Guid companyId, Guid customerId, RecordCustomerPaymentDto dto, Guid actingUserId);
}

public interface ISupplierService
{
    Task<List<SupplierListItemDto>> GetAllAsync(Guid companyId);
    Task<SupplierLedgerDto?> GetLedgerAsync(Guid companyId, Guid supplierId);
    Task<Guid> CreateAsync(Guid companyId, CreateSupplierDto dto);
    Task<bool> UpdateAsync(Guid companyId, Guid id, UpdateSupplierDto dto);
    Task<bool> RecordPaymentAsync(Guid companyId, Guid supplierId, RecordSupplierPaymentDto dto, Guid actingUserId);
}
