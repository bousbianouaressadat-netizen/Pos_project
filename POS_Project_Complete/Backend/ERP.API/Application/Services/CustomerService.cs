using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using ERP.API.Domain.Entities;
using ERP.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _db;
    public CustomerService(AppDbContext db) => _db = db;

    public async Task<List<CustomerListItemDto>> GetAllAsync(Guid companyId)
    {
        var customers = await _db.Customers
            .Where(c => c.CompanyID == companyId)
            .Include(c => c.Transactions)
            .ToListAsync();

        return customers.Select(c => new CustomerListItemDto(
            c.CustomerID, c.Name, c.Phone,
            CalculateBalance(c), c.IsActive
        )).ToList();
    }

    public async Task<CustomerLedgerDto?> GetLedgerAsync(Guid companyId, Guid customerId)
    {
        var customer = await _db.Customers
            .Include(c => c.Transactions)
            .FirstOrDefaultAsync(c => c.CompanyID == companyId && c.CustomerID == customerId);

        if (customer is null) return null;

        var transactions = customer.Transactions
            .OrderByDescending(t => t.Timestamp)
            .Select(t => new CustomerTransactionDto(
                t.CustomerTransactionID, t.Type, t.Amount, t.ReferenceType, t.ReferenceID, t.Timestamp))
            .ToList();

        return new CustomerLedgerDto(customer.CustomerID, customer.Name, CalculateBalance(customer), transactions);
    }

    public async Task<Guid> CreateAsync(Guid companyId, CreateCustomerDto dto)
    {
        var customer = new Customer
        {
            CompanyID = companyId,
            Name = dto.Name,
            Phone = dto.Phone,
            Address = dto.Address,
            PriceListID = dto.PriceListID,
            OpeningBalance = dto.OpeningBalance,
            IsActive = true
        };

        // الرصيد الافتتاحي يُسجَّل كحركة أولى بالـ Ledger، وليس حقلًا يُعدَّل يدويًا لاحقًا
        if (dto.OpeningBalance != 0)
        {
            customer.Transactions.Add(new CustomerTransaction
            {
                Type = "OpeningBalance",
                Amount = dto.OpeningBalance,
                Customer = customer
            });
        }

        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();
        return customer.CustomerID;
    }

    public async Task<bool> UpdateAsync(Guid companyId, Guid id, UpdateCustomerDto dto)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.CompanyID == companyId && c.CustomerID == id);
        if (customer is null) return false;

        customer.Name = dto.Name;
        customer.Phone = dto.Phone;
        customer.Address = dto.Address;
        customer.PriceListID = dto.PriceListID;
        customer.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RecordPaymentAsync(Guid companyId, Guid customerId, RecordCustomerPaymentDto dto, Guid actingUserId)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.CompanyID == companyId && c.CustomerID == customerId);
        if (customer is null) return false;

        if (dto.Amount <= 0) throw new InvalidOperationException("مبلغ الدفعة يجب أن يكون أكبر من صفر");

        // دفعة العميل تُخفّض الدين → قيمة سالبة بالـ Ledger
        _db.CustomerTransactions.Add(new CustomerTransaction
        {
            CustomerID = customerId,
            Type = "Payment",
            Amount = -dto.Amount
        });

        await _db.SaveChangesAsync();
        return true;
    }

    private static decimal CalculateBalance(Customer customer)
        => customer.Transactions.Sum(t => t.Amount);
}
