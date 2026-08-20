using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using ERP.API.Domain.Entities;
using ERP.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Application.Services;

public class SupplierService : ISupplierService
{
    private readonly AppDbContext _db;
    public SupplierService(AppDbContext db) => _db = db;

    public async Task<List<SupplierListItemDto>> GetAllAsync(Guid companyId)
    {
        var suppliers = await _db.Suppliers
            .Where(s => s.CompanyID == companyId)
            .Include(s => s.Transactions)
            .ToListAsync();

        return suppliers.Select(s => new SupplierListItemDto(
            s.SupplierID, s.Name, s.Phone, CalculateBalance(s), s.IsActive
        )).ToList();
    }

    public async Task<SupplierLedgerDto?> GetLedgerAsync(Guid companyId, Guid supplierId)
    {
        var supplier = await _db.Suppliers
            .Include(s => s.Transactions)
            .FirstOrDefaultAsync(s => s.CompanyID == companyId && s.SupplierID == supplierId);

        if (supplier is null) return null;

        var transactions = supplier.Transactions
            .OrderByDescending(t => t.Timestamp)
            .Select(t => new SupplierTransactionDto(
                t.SupplierTransactionID, t.Type, t.Amount, t.ReferenceType, t.ReferenceID, t.Timestamp))
            .ToList();

        return new SupplierLedgerDto(supplier.SupplierID, supplier.Name, CalculateBalance(supplier), transactions);
    }

    public async Task<Guid> CreateAsync(Guid companyId, CreateSupplierDto dto)
    {
        var supplier = new Supplier
        {
            CompanyID = companyId,
            Name = dto.Name,
            Phone = dto.Phone,
            Address = dto.Address,
            OpeningBalance = dto.OpeningBalance,
            IsActive = true
        };

        if (dto.OpeningBalance != 0)
        {
            supplier.Transactions.Add(new SupplierTransaction
            {
                Type = "OpeningBalance",
                Amount = dto.OpeningBalance,
                Supplier = supplier
            });
        }

        _db.Suppliers.Add(supplier);
        await _db.SaveChangesAsync();
        return supplier.SupplierID;
    }

    public async Task<bool> UpdateAsync(Guid companyId, Guid id, UpdateSupplierDto dto)
    {
        var supplier = await _db.Suppliers.FirstOrDefaultAsync(s => s.CompanyID == companyId && s.SupplierID == id);
        if (supplier is null) return false;

        supplier.Name = dto.Name;
        supplier.Phone = dto.Phone;
        supplier.Address = dto.Address;
        supplier.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RecordPaymentAsync(Guid companyId, Guid supplierId, RecordSupplierPaymentDto dto, Guid actingUserId)
    {
        var supplier = await _db.Suppliers.FirstOrDefaultAsync(s => s.CompanyID == companyId && s.SupplierID == supplierId);
        if (supplier is null) return false;

        if (dto.Amount <= 0) throw new InvalidOperationException("مبلغ الدفعة يجب أن يكون أكبر من صفر");

        // دفعنا للمورد تُخفّض ما نداينه به → قيمة سالبة بالـ Ledger
        _db.SupplierTransactions.Add(new SupplierTransaction
        {
            SupplierID = supplierId,
            Type = "Payment",
            Amount = -dto.Amount
        });

        await _db.SaveChangesAsync();
        return true;
    }

    private static decimal CalculateBalance(Supplier supplier)
        => supplier.Transactions.Sum(t => t.Amount);
}
