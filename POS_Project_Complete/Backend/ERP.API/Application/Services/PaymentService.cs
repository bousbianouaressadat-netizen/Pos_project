using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using ERP.API.Domain.Entities;
using ERP.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _db;
    public PaymentService(AppDbContext db) => _db = db;

    public async Task<Guid> RecordPaymentAsync(Guid companyId, RecordPaymentDto dto, Guid userId)
    {
        if (dto.Amount <= 0) throw new InvalidOperationException("المبلغ يجب أن يكون أكبر من صفر");

        var payment = new Payment
        {
            CompanyID = companyId,
            Direction = dto.Direction,
            Method = dto.Method,
            Amount = dto.Amount,
            ReferenceType = dto.ReferenceType,
            ReferenceID = dto.ReferenceID,
            CustomerID = dto.CustomerID,
            SupplierID = dto.SupplierID,
            UserID = userId
        };

        _db.Payments.Add(payment);

        // تحديث Ledger العميل/المورد تلقائيًا لو الدفعة مرتبطة بأحدهم مباشرة
        // (تحصيل دين عميل أو دفع لمورد خارج سياق فاتورة جديدة)
        if (dto.CustomerID.HasValue && dto.ReferenceType != nameof(SaleInvoice))
        {
            _db.CustomerTransactions.Add(new CustomerTransaction
            {
                CustomerID = dto.CustomerID.Value,
                Type = "Payment",
                Amount = dto.Direction == "In" ? -dto.Amount : dto.Amount,
                ReferenceType = "Payment",
                ReferenceID = payment.PaymentID
            });
        }

        if (dto.SupplierID.HasValue && dto.ReferenceType != nameof(PurchaseInvoice))
        {
            _db.SupplierTransactions.Add(new SupplierTransaction
            {
                SupplierID = dto.SupplierID.Value,
                Type = "Payment",
                Amount = dto.Direction == "Out" ? -dto.Amount : dto.Amount,
                ReferenceType = "Payment",
                ReferenceID = payment.PaymentID
            });
        }

        await _db.SaveChangesAsync();
        return payment.PaymentID;
    }

    public async Task<List<PaymentDto>> GetAllAsync(Guid companyId, DateTime? from, DateTime? to)
    {
        var query = _db.Payments.Where(p => p.CompanyID == companyId);
        if (from.HasValue) query = query.Where(p => p.Timestamp >= from.Value);
        if (to.HasValue) query = query.Where(p => p.Timestamp <= to.Value);

        return await query
            .Select(p => new PaymentDto(p.PaymentID, p.Direction, p.Method, p.Amount, p.ReferenceType, p.ReferenceID, p.Timestamp))
            .ToListAsync();
    }
}
