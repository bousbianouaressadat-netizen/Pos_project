namespace ERP.API.Domain.Entities;

public class Category
{
    public Guid CategoryID { get; set; } = Guid.NewGuid();
    public Guid CompanyID { get; set; }
    public Guid? ParentCategoryID { get; set; }
    public Category? ParentCategory { get; set; }

    public string NameAR { get; set; } = string.Empty;
    public string NameFR { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Unit
{
    public Guid UnitID { get; set; } = Guid.NewGuid();
    public Guid CompanyID { get; set; }

    public string NameAR { get; set; } = string.Empty;
    public string NameFR { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
