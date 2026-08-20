namespace ERP.API.Application.DTOs;

public record DashboardDto(
    decimal TodaySales, int TodayInvoicesCount, decimal TodayCOGS,
    decimal GrossProfitToday, decimal ExpensesToday, decimal NetProfitToday,
    decimal TotalCollectionsToday,
    decimal TotalCustomerDebts, decimal TotalSupplierDebts,
    decimal CurrentCashBalance,
    List<TopProductDto> TopSellingProducts,
    List<LowStockProductDto> LowStockProducts,
    List<RecentOperationDto> RecentOperations
);

public record TopProductDto(Guid ProductID, string NameAR, decimal QtySold, decimal Revenue);
public record LowStockProductDto(Guid ProductID, string NameAR, decimal CurrentQuantity, int MinStock);
public record RecentOperationDto(string Type, string Description, decimal? Amount, DateTime Timestamp);

public record SalesReportLineDto(DateTime Date, decimal TotalSales, int InvoicesCount);
public record SalesReportDto(decimal TotalSales, int TotalInvoices, List<SalesReportLineDto> ByDay);

public record ProfitByProductDto(Guid ProductID, string NameAR, decimal Revenue, decimal COGS, decimal GrossProfit);
public record ProfitReportDto(decimal TotalRevenue, decimal TotalCOGS, decimal TotalGrossProfit, decimal TotalExpenses, decimal NetProfit, List<ProfitByProductDto> ByProduct);

public record StockReportLineDto(Guid ProductID, string NameAR, decimal CurrentQuantity, decimal StockValue);
public record StockReportDto(decimal TotalStockValue, List<StockReportLineDto> Lines);
