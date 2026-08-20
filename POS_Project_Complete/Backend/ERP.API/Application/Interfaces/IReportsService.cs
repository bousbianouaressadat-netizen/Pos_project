using ERP.API.Application.DTOs;

namespace ERP.API.Application.Interfaces;

public interface IReportsService
{
    Task<DashboardDto> GetDashboardAsync(Guid companyId);
    Task<SalesReportDto> GetSalesReportAsync(Guid companyId, DateTime from, DateTime to);
    Task<ProfitReportDto> GetProfitReportAsync(Guid companyId, DateTime from, DateTime to);
    Task<StockReportDto> GetStockReportAsync(Guid companyId);
}
