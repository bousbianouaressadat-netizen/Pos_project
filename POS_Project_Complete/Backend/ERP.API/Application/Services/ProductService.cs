using System.Text.Json;
using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using ERP.API.Domain.Entities;
using ERP.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Application.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _db;
    public ProductService(AppDbContext db) => _db = db;

    public async Task<List<ProductListItemDto>> GetAllAsync(Guid companyId, string? search)
    {
        var query = _db.Products
            .Where(p => p.CompanyID == companyId)
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p =>
                p.NameAR.Contains(search) ||
                p.NameFR.Contains(search) ||
                p.SKU.Contains(search) ||
                p.Barcodes.Any(b => b.Code.Contains(search)));
        }

        return await query
            .Select(p => new ProductListItemDto(
                p.ProductID, p.SKU, p.NameAR, p.NameFR,
                p.Category != null ? p.Category.NameAR : null,
                p.Unit != null ? p.Unit.Symbol : null,
                p.Price, p.TaxRate, p.IsActive))
            .ToListAsync();
    }

    public async Task<ProductDetailDto?> GetByIdAsync(Guid companyId, Guid productId)
    {
        var p = await _db.Products
            .Include(x => x.Barcodes)
            .FirstOrDefaultAsync(x => x.CompanyID == companyId && x.ProductID == productId);

        if (p is null) return null;

        return new ProductDetailDto(
            p.ProductID, p.SKU, p.NameAR, p.NameFR, p.CategoryID, p.Brand, p.UnitID,
            p.PurchasePrice, p.CostPrice, p.Price, p.TaxRate,
            p.MinStock, p.MaxStock, p.ImagePath, p.IsActive,
            p.Barcodes.Select(b => b.Code).ToList()
        );
    }

    public async Task<ProductByBarcodeDto?> GetByBarcodeAsync(Guid companyId, string barcode)
    {
        var p = await _db.Products
            .Where(x => x.CompanyID == companyId && x.IsActive)
            .FirstOrDefaultAsync(x => x.Barcodes.Any(b => b.Code == barcode) || x.SKU == barcode);

        return p is null ? null : new ProductByBarcodeDto(p.ProductID, p.SKU, p.NameAR, p.NameFR, p.Price, p.TaxRate);
    }

    public async Task<Guid> CreateAsync(Guid companyId, CreateProductDto dto, Guid actingUserId)
    {
        var skuExists = await _db.Products.AnyAsync(p => p.CompanyID == companyId && p.SKU == dto.SKU);
        if (skuExists) throw new InvalidOperationException("رمز المنتج (SKU) موجود مسبقًا");

        if (dto.Barcodes is not null && dto.Barcodes.Count > 0)
        {
            var duplicates = await _db.ProductBarcodes.AnyAsync(b => dto.Barcodes.Contains(b.Code));
            if (duplicates) throw new InvalidOperationException("أحد الباركودات مستخدم مسبقًا لمنتج آخر");
        }

        var product = new Product
        {
            CompanyID = companyId,
            SKU = dto.SKU,
            NameAR = dto.NameAR,
            NameFR = dto.NameFR,
            CategoryID = dto.CategoryID,
            Brand = dto.Brand,
            UnitID = dto.UnitID,
            PurchasePrice = dto.PurchasePrice,
            CostPrice = dto.CostPrice,
            Price = dto.Price,
            TaxRate = dto.TaxRate,
            MinStock = dto.MinStock,
            MaxStock = dto.MaxStock,
            IsActive = true
        };

        if (dto.Barcodes is not null)
        {
            foreach (var code in dto.Barcodes.Distinct())
                product.Barcodes.Add(new ProductBarcode { Code = code, Type = "EAN13", Product = product });
        }

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        await LogAudit(actingUserId, nameof(Product), product.ProductID.ToString(), "Create",
            null, JsonSerializer.Serialize(new { product.SKU, product.NameAR, product.Price }));

        return product.ProductID;
    }

    public async Task<bool> UpdateAsync(Guid companyId, Guid productId, UpdateProductDto dto, Guid actingUserId)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.CompanyID == companyId && p.ProductID == productId);
        if (product is null) return false;

        var oldValue = JsonSerializer.Serialize(new { product.Price, product.CostPrice, product.IsActive });

        product.NameAR = dto.NameAR;
        product.NameFR = dto.NameFR;
        product.CategoryID = dto.CategoryID;
        product.Brand = dto.Brand;
        product.UnitID = dto.UnitID;
        product.PurchasePrice = dto.PurchasePrice;
        product.CostPrice = dto.CostPrice;
        product.Price = dto.Price;
        product.TaxRate = dto.TaxRate;
        product.MinStock = dto.MinStock;
        product.MaxStock = dto.MaxStock;
        product.IsActive = dto.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await LogAudit(actingUserId, nameof(Product), productId.ToString(), "Update",
            oldValue, JsonSerializer.Serialize(new { dto.Price, dto.CostPrice, dto.IsActive }));

        return true;
    }

    public async Task<bool> DeactivateAsync(Guid companyId, Guid productId, Guid actingUserId)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.CompanyID == companyId && p.ProductID == productId);
        if (product is null) return false;

        product.IsActive = false;
        await _db.SaveChangesAsync();

        await LogAudit(actingUserId, nameof(Product), productId.ToString(), "Deactivate", null, null);
        return true;
    }

    private async Task LogAudit(Guid userId, string entity, string entityId, string action, string? oldVal, string? newVal)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            UserID = userId,
            EntityName = entity,
            EntityID = entityId,
            Action = action,
            OldValue = oldVal,
            NewValue = newVal
        });
        await _db.SaveChangesAsync();
    }
}
