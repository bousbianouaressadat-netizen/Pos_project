using ERP.API.Application.DTOs;

namespace ERP.API.Application.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync(Guid companyId);
    Task<Guid> CreateAsync(Guid companyId, CreateCategoryDto dto);
    Task<bool> UpdateAsync(Guid companyId, Guid id, UpdateCategoryDto dto);
    Task<bool> DeactivateAsync(Guid companyId, Guid id);
}

public interface IUnitService
{
    Task<List<UnitDto>> GetAllAsync(Guid companyId);
    Task<Guid> CreateAsync(Guid companyId, CreateUnitDto dto);
    Task<bool> UpdateAsync(Guid companyId, Guid id, UpdateUnitDto dto);
}

public interface IProductService
{
    Task<List<ProductListItemDto>> GetAllAsync(Guid companyId, string? search);
    Task<ProductDetailDto?> GetByIdAsync(Guid companyId, Guid productId);
    Task<ProductByBarcodeDto?> GetByBarcodeAsync(Guid companyId, string barcode);
    Task<Guid> CreateAsync(Guid companyId, CreateProductDto dto, Guid actingUserId);
    Task<bool> UpdateAsync(Guid companyId, Guid productId, UpdateProductDto dto, Guid actingUserId);
    Task<bool> DeactivateAsync(Guid companyId, Guid productId, Guid actingUserId);
}
