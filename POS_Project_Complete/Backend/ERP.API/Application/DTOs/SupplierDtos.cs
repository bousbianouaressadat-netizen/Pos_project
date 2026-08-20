namespace ERP.API.Application.DTOs;

public record SupplierListItemDto(Guid SupplierID, string Name, string? Phone, decimal CurrentBalance, bool IsActive);

public record CreateSupplierDto(string Name, string? Phone, string? Address, decimal OpeningBalance);

public record UpdateSupplierDto(string Name, string? Phone, string? Address, bool IsActive);

public record SupplierTransactionDto(
    Guid SupplierTransactionID, string Type, decimal Amount,
    string? ReferenceType, Guid? ReferenceID, DateTime Timestamp
);

public record SupplierLedgerDto(Guid SupplierID, string Name, decimal CurrentBalance, List<SupplierTransactionDto> Transactions);

public record RecordSupplierPaymentDto(decimal Amount, string? Note);
