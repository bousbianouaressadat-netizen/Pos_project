namespace ERP.API.Application.DTOs;

public record RecordPaymentDto(
    string Direction, string Method, decimal Amount,
    string? ReferenceType, Guid? ReferenceID, Guid? CustomerID, Guid? SupplierID
);
public record PaymentDto(
    Guid PaymentID, string Direction, string Method, decimal Amount,
    string? ReferenceType, Guid? ReferenceID, DateTime Timestamp
);

public record OpenCashSessionDto(decimal OpeningBalance);
public record CloseCashSessionDto(decimal ActualCash);
public record CashSessionDto(
    Guid CashSessionID, decimal OpeningBalance, decimal? ExpectedCash,
    decimal? ActualCash, decimal? Difference, DateTime OpenedAt, DateTime? ClosedAt, string Status
);

public record CreateExpenseDto(string Category, decimal Amount, string? Description);
public record ExpenseDto(Guid ExpenseID, string Category, decimal Amount, string? Description, DateTime Timestamp);
