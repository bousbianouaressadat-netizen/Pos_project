using ERP.API.Application.DTOs;

namespace ERP.API.Application.Interfaces;

public interface IPaymentService
{
    Task<Guid> RecordPaymentAsync(Guid companyId, RecordPaymentDto dto, Guid userId);
    Task<List<PaymentDto>> GetAllAsync(Guid companyId, DateTime? from, DateTime? to);
}

public interface ICashSessionService
{
    Task<Guid> OpenAsync(Guid companyId, Guid userId, OpenCashSessionDto dto);
    Task<CashSessionDto?> CloseAsync(Guid companyId, Guid sessionId, CloseCashSessionDto dto);
    Task<CashSessionDto?> GetCurrentOpenAsync(Guid companyId, Guid userId);
    Task<List<CashSessionDto>> GetHistoryAsync(Guid companyId);
}

public interface IExpenseService
{
    Task<Guid> CreateAsync(Guid companyId, CreateExpenseDto dto, Guid userId);
    Task<List<ExpenseDto>> GetAllAsync(Guid companyId, DateTime? from, DateTime? to);
}
