using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using ERP.API.Domain.Entities;
using ERP.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Application.Services;

public class CashSessionService : ICashSessionService
{
    private readonly AppDbContext _db;
    public CashSessionService(AppDbContext db) => _db = db;

    public async Task<Guid> OpenAsync(Guid companyId, Guid userId, OpenCashSessionDto dto)
    {
        var alreadyOpen = await _db.CashSessions
            .AnyAsync(c => c.CompanyID == companyId && c.UserID == userId && c.Status == "Open");

        if (alreadyOpen) throw new InvalidOperationException("يوجد جلسة صندوق مفتوحة بالفعل لهذا المستخدم");

        var session = new CashSession
        {
            CompanyID = companyId,
            UserID = userId,
            OpeningBalance = dto.OpeningBalance,
            Status = "Open"
        };

        _db.CashSessions.Add(session);
        await _db.SaveChangesAsync();
        return session.CashSessionID;
    }

    /// <summary>
    /// المعادلة (Business Rule 6):
    /// Expected = Opening + CashSales + CustomerCashPayments + OtherIncome - Expenses - SupplierCashPayments - Withdrawals
    /// </summary>
    public async Task<CashSessionDto?> CloseAsync(Guid companyId, Guid sessionId, CloseCashSessionDto dto)
    {
        var session = await _db.CashSessions
            .FirstOrDefaultAsync(c => c.CompanyID == companyId && c.CashSessionID == sessionId && c.Status == "Open");

        if (session is null) return null;

        var periodStart = session.OpenedAt;
        var periodEnd = DateTime.UtcNow;

        var cashSales = await _db.SaleInvoices
            .Where(i => i.CompanyID == companyId && i.Status == "Completed"
                        && i.PaymentMethod == "Cash" && i.CreatedAt >= periodStart && i.CreatedAt <= periodEnd)
            .SumAsync(i => (decimal?)i.PaidAmount) ?? 0;

        var cashPaymentsIn = await _db.Payments
            .Where(p => p.CompanyID == companyId && p.Direction == "In" && p.Method == "Cash"
                        && p.Timestamp >= periodStart && p.Timestamp <= periodEnd)
            .SumAsync(p => (decimal?)p.Amount) ?? 0;

        var cashPaymentsOut = await _db.Payments
            .Where(p => p.CompanyID == companyId && p.Direction == "Out" && p.Method == "Cash"
                        && p.Timestamp >= periodStart && p.Timestamp <= periodEnd)
            .SumAsync(p => (decimal?)p.Amount) ?? 0;

        var expenses = await _db.Expenses
            .Where(e => e.CompanyID == companyId && e.Timestamp >= periodStart && e.Timestamp <= periodEnd)
            .SumAsync(e => (decimal?)e.Amount) ?? 0;

        var expected = session.OpeningBalance + cashSales + cashPaymentsIn - expenses - cashPaymentsOut;

        session.ExpectedCash = expected;
        session.ActualCash = dto.ActualCash;
        session.Difference = dto.ActualCash - expected;
        session.Status = "Closed";
        session.ClosedAt = periodEnd;

        await _db.SaveChangesAsync();

        return ToDto(session);
    }

    public async Task<CashSessionDto?> GetCurrentOpenAsync(Guid companyId, Guid userId)
    {
        var session = await _db.CashSessions
            .FirstOrDefaultAsync(c => c.CompanyID == companyId && c.UserID == userId && c.Status == "Open");

        return session is null ? null : ToDto(session);
    }

    public async Task<List<CashSessionDto>> GetHistoryAsync(Guid companyId)
    {
        return await _db.CashSessions
            .Where(c => c.CompanyID == companyId)
            .OrderByDescending(c => c.OpenedAt)
            .Select(c => new CashSessionDto(
                c.CashSessionID, c.OpeningBalance, c.ExpectedCash, c.ActualCash, c.Difference,
                c.OpenedAt, c.ClosedAt, c.Status))
            .ToListAsync();
    }

    private static CashSessionDto ToDto(CashSession c) => new(
        c.CashSessionID, c.OpeningBalance, c.ExpectedCash, c.ActualCash, c.Difference,
        c.OpenedAt, c.ClosedAt, c.Status);
}
