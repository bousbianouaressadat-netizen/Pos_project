namespace ERP.API.Application.DTOs;

public record CustomerListItemDto(Guid CustomerID, string Name, string? Phone, decimal CurrentBalance, bool IsActive);

public record CreateCustomerDto(string Name, string? Phone, string? Address, Guid? PriceListID, decimal OpeningBalance);

public record UpdateCustomerDto(string Name, string? Phone, string? Address, Guid? PriceListID, bool IsActive);

public record CustomerTransactionDto(
    Guid CustomerTransactionID, string Type, decimal Amount,
    string? ReferenceType, Guid? ReferenceID, DateTime Timestamp
);

public record CustomerLedgerDto(Guid CustomerID, string Name, decimal CurrentBalance, List<CustomerTransactionDto> Transactions);

public record RecordCustomerPaymentDto(decimal Amount, string? Note);
