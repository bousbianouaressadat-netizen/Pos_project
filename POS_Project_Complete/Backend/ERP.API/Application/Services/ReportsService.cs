using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using ERP.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Application.Services;

public class ReportsService : IReportsService
{
    private readonly AppDbContext _db;
    public ReportsService(AppDbContext db) => _db = db;

    public async Task<DashboardDto> GetDashboardAsync(Guid companyId)
    {
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1);

        var todayInvoices = await _db.SaleInvoices
            .Where(i => i.CompanyID == companyId && i.Status == "Completed"
                        && i.CreatedAt >= todayStart && i.CreatedAt < todayEnd)
            .ToListAsync();

        var todaySales = todayInvoices.Sum(i => i.TotalAmount);
        var todayInvoicesCount = todayInvoices.Count;

        var todayLines = await _db.SaleInvoiceLines
            .Where(l => todayInvoices.Select(i => i.SaleInvoiceID).Contains(l.SaleInvoiceID))
            .Include(l => l.SaleInvoice)
            .ToListAsync();

        var productCosts = await _db.Products
            .Where(p => p.CompanyID == companyId)
            .ToDictionaryAsync(p => p.ProductID, p => p.CostPrice);

        var todayCOGS = todayLines.Sum(l => l.Qty * productCosts.GetValueOrDefault(l.ProductID, 0));
        var grossProfitToday = todaySales - todayCOGS;

        var expensesToday = await _db.Expenses
            .Where(e => e.CompanyID == companyId && e.Timestamp >= todayStart && e.Timestamp < todayEnd)
            .SumAsync(e => (decimal?)e.Amount) ?? 0;

        var netProfitToday = grossProfitToday - expensesToday;

        var collectionsToday = await _db.Payments
            .Where(p => p.CompanyID == companyId && p.Direction == "In"
                        && p.Timestamp >= todayStart && p.Timestamp < todayEnd)
            .SumAsync(p => (decimal?)p.Amount) ?? 0;

        var totalCustomerDebts = await _db.CustomerTransactions
            .Where(t => t.Customer!.CompanyID == companyId)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        var totalSupplierDebts = await _db.SupplierTransactions
            .Where(t => t.Supplier!.CompanyID == companyId)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        var cashIn = await _db.Payments.Where(p => p.CompanyID == companyId && p.Direction == "In" && p.Method == "Cash")
            .SumAsync(p => (decimal?)p.Amount) ?? 0;
        var cashOut = await _db.Payments.Where(p => p.CompanyID == companyId && p.Direction == "Out" && p.Method == "Cash")
            .SumAsync(p => (decimal?)p.Amount) ?? 0;
        var allExpenses = await _db.Expenses.Where(e => e.CompanyID == companyId).SumAsync(e => (decimal?)e.Amount) ?? 0;
        var currentCashBalance = cashIn - cashOut - allExpenses;

        var topProducts = await _db.SaleInvoiceLines
            .Where(l => l.SaleInvoice!.CompanyID == companyId && l.SaleInvoice.Status == "Completed")
            .GroupBy(l => l.ProductID)
            .Select(g => new { ProductID = g.Key, Qty = g.Sum(x => x.Qty), Revenue = g.Sum(x => x.Qty * x.UnitPrice) })
            .OrderByDescending(x => x.Qty)
            .Take(5)
            .ToListAsync();

        var productNames = await _db.Products.ToDictionaryAsync(p => p.ProductID, p => p.NameAR);

        var topProductDtos = topProducts
            .Select(x => new TopProductDto(x.ProductID, productNames.GetValueOrDefault(x.ProductID, ""), x.Qty, x.Revenue))
            .ToList();

        var lowStock = await _db.StockBalances
            .Join(_db.Products.Where(p => p.CompanyID == companyId), b => b.ProductID, p => p.ProductID, (b, p) => new { b, p })
            .Where(x => x.b.CurrentQuantity <= x.p.MinStock)
            .Select(x => new LowStockProductDto(x.p.ProductID, x.p.NameAR, x.b.CurrentQuantity, x.p.MinStock))
            .Take(10)
            .ToListAsync();

        var recentOps = todayInvoices
            .OrderByDescending(i => i.CreatedAt)
            .Take(10)
            .Select(i => new RecentOperationDto("Sale", $"فاتورة بيع #{i.SaleInvoiceID.ToString()[..8]}", i.TotalAmount, i.CreatedAt))
            .ToList();

        return new DashboardDto(
            todaySales, todayInvoicesCount, todayCOGS, grossProfitToday, expensesToday, netProfitToday,
            collectionsToday, totalCustomerDebts, totalSupplierDebts, currentCashBalance,
            topProductDtos, lowStock, recentOps
        );
    }

    public async Task<SalesReportDto> GetSalesReportAsync(Guid companyId, DateTime from, DateTime to)
    {
        var invoices = await _db.SaleInvoices
            .Where(i => i.CompanyID == companyId && i.Status == "Completed" && i.CreatedAt >= from && i.CreatedAt <= to)
            .ToListAsync();

        var byDay = invoices
            .GroupBy(i => i.CreatedAt.Date)
            .Select(g => new SalesReportLineDto(g.Key, g.Sum(i => i.TotalAmount), g.Count()))
            .OrderBy(x => x.Date)
            .ToList();

        return new SalesReportDto(invoices.Sum(i => i.TotalAmount), invoices.Count, byDay);
    }

    public async Task<ProfitReportDto> GetProfitReportAsync(Guid companyId, DateTime from, DateTime to)
    {
        var lines = await _db.SaleInvoiceLines
            .Where(l => l.SaleInvoice!.CompanyID == companyId && l.SaleInvoice.Status == "Completed"
                        && l.SaleInvoice.CreatedAt >= from && l.SaleInvoice.CreatedAt <= to)
            .ToListAsync();

        var products = await _db.Products.Where(p => p.CompanyID == companyId).ToListAsync();
        var productDict = products.ToDictionary(p => p.ProductID);

        var byProduct = lines
            .GroupBy(l => l.ProductID)
            .Select(g =>
            {
                var revenue = g.Sum(x => x.Qty * x.UnitPrice - x.DiscountAmount);
                var cost = productDict.TryGetValue(g.Key, out var p) ? p.CostPrice : 0;
                var cogs = g.Sum(x => x.Qty) * cost;
                var name = productDict.TryGetValue(g.Key, out var p2) ? p2.NameAR : "";
                return new ProfitByProductDto(g.Key, name, revenue, cogs, revenue - cogs);
            })
            .OrderByDescending(x => x.GrossProfit)
            .ToList();

        var totalRevenue = byProduct.Sum(x => x.Revenue);
        var totalCOGS = byProduct.Sum(x => x.COGS);
        var totalGrossProfit = totalRevenue - totalCOGS;

        var totalExpenses = await _db.Expenses
            .Where(e => e.CompanyID == companyId && e.Timestamp >= from && e.Timestamp <= to)
            .SumAsync(e => (decimal?)e.Amount) ?? 0;

        return new ProfitReportDto(totalRevenue, totalCOGS, totalGrossProfit, totalExpenses, totalGrossProfit - totalExpenses, byProduct);
    }

    public async Task<StockReportDto> GetStockReportAsync(Guid companyId)
    {
        var products = await _db.Products.Where(p => p.CompanyID == companyId).ToDictionaryAsync(p => p.ProductID);

        var balances = await _db.StockBalances
            .Where(b => products.Keys.Contains(b.ProductID))
            .ToListAsync();

        var lines = balances
            .GroupBy(b => b.ProductID)
            .Select(g =>
            {
                var qty = g.Sum(x => x.CurrentQuantity);
                var cost = products.TryGetValue(g.Key, out var p) ? p.CostPrice : 0;
                var name = products.TryGetValue(g.Key, out var p2) ? p2.NameAR : "";
                return new StockReportLineDto(g.Key, name, qty, qty * cost);
            })
            .ToList();

        return new StockReportDto(lines.Sum(l => l.StockValue), lines);
    }
}
