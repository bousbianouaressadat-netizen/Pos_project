namespace ERP.API.Domain.Entities;

public class Product
{
    public Guid ProductID { get; set; } = Guid.NewGuid();
    public Guid CompanyID { get; set; }

    public string SKU { get; set; } = string.Empty;
    public string NameAR { get; set; } = string.Empty;
    public string NameFR { get; set; } = string.Empty;

    public Guid? CategoryID { get; set; }
    public Category? Category { get; set; }

    public string? Brand { get; set; }

    public Guid UnitID { get; set; }
    public Unit? Unit { get; set; }

    public decimal PurchasePrice { get; set; }
    public decimal CostPrice { get; set; }
    public decimal Price { get; set; }
    public decimal TaxRate { get; set; } = 19; // نسبة TVA افتراضية، قابلة للتعديل بالإعدادات

    public int MinStock { get; set; }
    public int MaxStock { get; set; }

    public string? ImagePath { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<ProductBarcode> Barcodes { get; set; } = new List<ProductBarcode>();
    public ICollection<ProductPrice> Prices { get; set; } = new List<ProductPrice>();
}

public class ProductBarcode
{
    public Guid BarcodeID { get; set; } = Guid.NewGuid();
    public Guid ProductID { get; set; }
    public Product? Product { get; set; }

    public string Code { get; set; } = string.Empty;
    public string Type { get; set; } = "EAN13"; // EAN13, EAN8, UPC, Code128, Internal
}

public class PriceList
{
    public Guid PriceListID { get; set; } = Guid.NewGuid();
    public Guid CompanyID { get; set; }

    public string Name { get; set; } = string.Empty; // Retail, SemiWholesale, Wholesale, Promotional, VIP...
    public bool IsDefault { get; set; }

    public ICollection<ProductPrice> ProductPrices { get; set; } = new List<ProductPrice>();
}

public class ProductPrice
{
    public Guid ProductPriceID { get; set; } = Guid.NewGuid();

    public Guid ProductID { get; set; }
    public Product? Product { get; set; }

    public Guid PriceListID { get; set; }
    public PriceList? PriceList { get; set; }

    public decimal Price { get; set; }
}
