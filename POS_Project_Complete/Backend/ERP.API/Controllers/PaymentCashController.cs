using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using ERP.API.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[Route("api/payments")]
public class PaymentsController : ApiControllerBase
{
    private readonly IPaymentService _service;
    public PaymentsController(IPaymentService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<PaymentDto>>> GetAll([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        => Ok(await _service.GetAllAsync(CompanyId, from, to));

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] RecordPaymentDto dto)
    {
        try
        {
            var id = await _service.RecordPaymentAsync(CompanyId, dto, CurrentUserId);
            return Ok(new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

[Route("api/cash-sessions")]
public class CashSessionsController : ApiControllerBase
{
    private readonly ICashSessionService _service;
    public CashSessionsController(ICashSessionService service) => _service = service;

    [HttpGet("current")]
    public async Task<ActionResult<CashSessionDto>> GetCurrent()
    {
        var session = await _service.GetCurrentOpenAsync(CompanyId, CurrentUserId);
        return session is null ? NotFound() : Ok(session);
    }

    [HttpGet("history")]
    public async Task<ActionResult<List<CashSessionDto>>> GetHistory() => Ok(await _service.GetHistoryAsync(CompanyId));

    [HttpPost("open")]
    public async Task<ActionResult> Open([FromBody] OpenCashSessionDto dto)
    {
        try
        {
            var id = await _service.OpenAsync(CompanyId, CurrentUserId, dto);
            return Ok(new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/close")]
    [RequirePermission("CanCloseCash")]
    public async Task<ActionResult<CashSessionDto>> Close(Guid id, [FromBody] CloseCashSessionDto dto)
    {
        var result = await _service.CloseAsync(CompanyId, id, dto);
        return result is null ? NotFound() : Ok(result);
    }
}

[Route("api/expenses")]
public class ExpensesController : ApiControllerBase
{
    private readonly IExpenseService _service;
    public ExpensesController(IExpenseService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<ExpenseDto>>> GetAll([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        => Ok(await _service.GetAllAsync(CompanyId, from, to));

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateExpenseDto dto)
    {
        var id = await _service.CreateAsync(CompanyId, dto, CurrentUserId);
        return Ok(new { id });
    }
}
