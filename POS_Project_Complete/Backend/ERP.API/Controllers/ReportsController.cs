using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using ERP.API.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[Route("api/reports")]
public class ReportsController : ApiControllerBase
{
    private readonly IReportsService _service;
    public ReportsController(IReportsService service) => _service = service;

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDto>> GetDashboard() => Ok(await _service.GetDashboardAsync(CompanyId));

    [HttpGet("sales")]
    [RequirePermission("CanViewReports")]
    public async Task<ActionResult<SalesReportDto>> GetSales([FromQuery] DateTime from, [FromQuery] DateTime to)
        => Ok(await _service.GetSalesReportAsync(CompanyId, from, to));

    [HttpGet("profit")]
    [RequirePermission("CanViewProfit")]
    public async Task<ActionResult<ProfitReportDto>> GetProfit([FromQuery] DateTime from, [FromQuery] DateTime to)
        => Ok(await _service.GetProfitReportAsync(CompanyId, from, to));

    [HttpGet("stock")]
    [RequirePermission("CanViewReports")]
    public async Task<ActionResult<StockReportDto>> GetStock() => Ok(await _service.GetStockReportAsync(CompanyId));
}
