using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using ERP.API.Domain.Entities;
using ERP.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Application.Services;

public class StockService : IStockService
{
    private readonly AppDbContext _db;
    public StockService(AppDbContext db) => _db = db;

    public async Task RecordMovementAsync(Guid productId, Guid warehouseId, decimal quantity, string movementType,
        string? referenceType, Guid? referenceId, Guid? userId)
    {
        _db.StockTransactions.Add(new StockTransaction
        {
            ProductID = productId,
            WarehouseID = warehouseId,
            Quantity = quantity,
            MovementType = movementType,
            ReferenceType = referenceType,
            ReferenceID = referenceId,
            UserID = userId
        });

        var balance = await _db.StockBalances
            .FirstOrDefaultAsync(b => b.ProductID == productId && b.WarehouseID == warehouseId);

        if (balance is null)
        {
            balance = new StockBalance { ProductID = productId, WarehouseID = warehouseId, CurrentQuantity = 0 };
            _db.StockBalances.Add(balance);
        }

        balance.CurrentQuantity += quantity;

        await _db.SaveChangesAsync();
    }

    public async Task<List<StockBalanceDto>> GetBalanceAsync(Guid companyId, Guid? warehouseId)
    {
        var query = _db.StockBalances
            .Join(_db.Products.Where(p => p.CompanyID == companyId), b => b.ProductID, p => p.ProductID,
                (b, p) => new { b, p })
            .Join(_db.Warehouses, x => x.b.WarehouseID, w => w.WarehouseID, (x, w) => new { x.b, x.p, w })
            .AsQueryable();

        if (warehouseId.HasValue)
            query = query.Where(x => x.b.WarehouseID == warehouseId.Value);

        return await query
            .Select(x => new StockBalanceDto(x.p.ProductID, x.p.NameAR, x.w.WarehouseID, x.w.Name, x.b.CurrentQuantity))
            .ToListAsync();
    }

    public async Task<List<StockTransactionDto>> GetTransactionsAsync(Guid companyId, Guid productId)
    {
        return await _db.StockTransactions
            .Where(t => t.ProductID == productId)
            .Include(t => t.Product)
            .OrderByDescending(t => t.Timestamp)
            .Select(t => new StockTransactionDto(
                t.StockTransactionID, t.ProductID, t.Product!.NameAR,
                t.MovementType, t.Quantity, t.ReferenceType, t.Timestamp))
            .ToListAsync();
    }

    public async Task CreateAdjustmentAsync(Guid companyId, CreateStockAdjustmentDto dto, Guid userId)
    {
        var adjustmentRefId = Guid.NewGuid();
        foreach (var line in dto.Lines)
        {
            await RecordMovementAsync(line.ProductID, dto.WarehouseID, line.Quantity,
                "Adjustment", "StockAdjustment", adjustmentRefId, userId);
        }
    }

    public async Task CreateTransferAsync(Guid companyId, CreateStockTransferDto dto, Guid userId)
    {
        if (dto.FromWarehouseID == dto.ToWarehouseID)
            throw new InvalidOperationException("لا يمكن التحويل لنفس المستودع");

        var transferRefId = Guid.NewGuid();
        foreach (var line in dto.Lines)
        {
            await RecordMovementAsync(line.ProductID, dto.FromWarehouseID, -line.Quantity,
                "Transfer", "StockTransfer", transferRefId, userId);
            await RecordMovementAsync(line.ProductID, dto.ToWarehouseID, line.Quantity,
                "Transfer", "StockTransfer", transferRefId, userId);
        }
    }
}
