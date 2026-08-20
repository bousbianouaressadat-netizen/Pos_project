namespace ERP.API.Application.DTOs;

public record ProductListItemDto(
    Guid ProductID, string SKU, string NameAR, string NameFR,
    string? CategoryNameAR, string? UnitSymbol,
    decimal Price, decimal TaxRate, bool IsActive
);

public record ProductDetailDto(
    Guid ProductID, string SKU, string NameAR, string NameFR,
    Guid? CategoryID, string? Brand, Guid UnitID,
    decimal PurchasePrice, decimal CostPrice, decimal Price, decimal TaxRate,
    int MinStock, int MaxStock, string? ImagePath, bool IsActive,
    List<string> Barcodes
);

public record CreateProductDto(
    string SKU, string NameAR, string NameFR,
    Guid? CategoryID, string? Brand, Guid UnitID,
    decimal PurchasePrice, decimal CostPrice, decimal Price, decimal TaxRate,
    int MinStock, int MaxStock,
    List<string>? Barcodes
);

public record UpdateProductDto(
    string NameAR, string NameFR,
    Guid? CategoryID, string? Brand, Guid UnitID,
    decimal PurchasePrice, decimal CostPrice, decimal Price, decimal TaxRate,
    int MinStock, int MaxStock, bool IsActive
);

public record ProductByBarcodeDto(
    Guid ProductID, string SKU, string NameAR, string NameFR,
    decimal Price, decimal TaxRate
);
