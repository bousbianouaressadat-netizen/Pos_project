using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using ERP.API.Domain.Entities;
using ERP.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Application.Services;

public class ExpenseService : IExpenseService
{
    private readonly AppDbContext _db;
    public ExpenseService(AppDbContext db) => _db = db;

    public async Task<Guid> CreateAsync(Guid companyId, CreateExpenseDto dto, Guid userId)
    {
        var expense = new Expense
        {
            CompanyID = companyId,
            Category = dto.Category,
            Amount = dto.Amount,
            Description = dto.Description,
            UserID = userId
        };

        _db.Expenses.Add(expense);
        await _db.SaveChangesAsync();
        return expense.ExpenseID;
    }

    public async Task<List<ExpenseDto>> GetAllAsync(Guid companyId, DateTime? from, DateTime? to)
    {
        var query = _db.Expenses.Where(e => e.CompanyID == companyId);
        if (from.HasValue) query = query.Where(e => e.Timestamp >= from.Value);
        if (to.HasValue) query = query.Where(e => e.Timestamp <= to.Value);

        return await query
            .Select(e => new ExpenseDto(e.ExpenseID, e.Category, e.Amount, e.Description, e.Timestamp))
            .ToListAsync();
    }
}
