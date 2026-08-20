namespace ERP.API.Application.DTOs;

public record CategoryDto(Guid CategoryID, string NameAR, string NameFR, Guid? ParentCategoryID, bool IsActive);
public record CreateCategoryDto(string NameAR, string NameFR, Guid? ParentCategoryID);
public record UpdateCategoryDto(string NameAR, string NameFR, Guid? ParentCategoryID, bool IsActive);

public record UnitDto(Guid UnitID, string NameAR, string NameFR, string Symbol);
public record CreateUnitDto(string NameAR, string NameFR, string Symbol);
public record UpdateUnitDto(string NameAR, string NameFR, string Symbol);
