using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using ERP.API.Domain.Entities;
using ERP.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _db;
    public CategoryService(AppDbContext db) => _db = db;

    public async Task<List<CategoryDto>> GetAllAsync(Guid companyId)
    {
        return await _db.Categories
            .Where(c => c.CompanyID == companyId)
            .Select(c => new CategoryDto(c.CategoryID, c.NameAR, c.NameFR, c.ParentCategoryID, c.IsActive))
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(Guid companyId, CreateCategoryDto dto)
    {
        var category = new Category
        {
            CompanyID = companyId,
            NameAR = dto.NameAR,
            NameFR = dto.NameFR,
            ParentCategoryID = dto.ParentCategoryID
        };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return category.CategoryID;
    }

    public async Task<bool> UpdateAsync(Guid companyId, Guid id, UpdateCategoryDto dto)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.CompanyID == companyId && c.CategoryID == id);
        if (category is null) return false;

        category.NameAR = dto.NameAR;
        category.NameFR = dto.NameFR;
        category.ParentCategoryID = dto.ParentCategoryID;
        category.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeactivateAsync(Guid companyId, Guid id)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.CompanyID == companyId && c.CategoryID == id);
        if (category is null) return false;

        category.IsActive = false;
        await _db.SaveChangesAsync();
        return true;
    }
}
