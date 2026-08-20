using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using ERP.API.Domain.Entities;
using ERP.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Application.Services;

public class UnitService : IUnitService
{
    private readonly AppDbContext _db;
    public UnitService(AppDbContext db) => _db = db;

    public async Task<List<UnitDto>> GetAllAsync(Guid companyId)
    {
        return await _db.Units
            .Where(u => u.CompanyID == companyId)
            .Select(u => new UnitDto(u.UnitID, u.NameAR, u.NameFR, u.Symbol))
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(Guid companyId, CreateUnitDto dto)
    {
        var unit = new Unit { CompanyID = companyId, NameAR = dto.NameAR, NameFR = dto.NameFR, Symbol = dto.Symbol };
        _db.Units.Add(unit);
        await _db.SaveChangesAsync();
        return unit.UnitID;
    }

    public async Task<bool> UpdateAsync(Guid companyId, Guid id, UpdateUnitDto dto)
    {
        var unit = await _db.Units.FirstOrDefaultAsync(u => u.CompanyID == companyId && u.UnitID == id);
        if (unit is null) return false;

        unit.NameAR = dto.NameAR;
        unit.NameFR = dto.NameFR;
        unit.Symbol = dto.Symbol;

        await _db.SaveChangesAsync();
        return true;
    }
}
