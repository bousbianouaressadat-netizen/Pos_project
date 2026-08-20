using ERP.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductBarcode> ProductBarcodes => Set<ProductBarcode>();
    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<ProductPrice> ProductPrices => Set<ProductPrice>();

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerTransaction> CustomerTransactions => Set<CustomerTransaction>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<SupplierTransaction> SupplierTransactions => Set<SupplierTransaction>();

    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
    public DbSet<StockBalance> StockBalances => Set<StockBalance>();

    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
    public DbSet<GoodsReceipt> GoodsReceipts => Set<GoodsReceipt>();
    public DbSet<GoodsReceiptLine> GoodsReceiptLines => Set<GoodsReceiptLine>();
    public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();
    public DbSet<PurchaseInvoiceLine> PurchaseInvoiceLines => Set<PurchaseInvoiceLine>();

    public DbSet<Quotation> Quotations => Set<Quotation>();
    public DbSet<QuotationLine> QuotationLines => Set<QuotationLine>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();
    public DbSet<DeliveryNote> DeliveryNotes => Set<DeliveryNote>();
    public DbSet<DeliveryNoteLine> DeliveryNoteLines => Set<DeliveryNoteLine>();
    public DbSet<SaleInvoice> SaleInvoices => Set<SaleInvoice>();
    public DbSet<SaleInvoiceLine> SaleInvoiceLines => Set<SaleInvoiceLine>();
    public DbSet<SaleReturn> SaleReturns => Set<SaleReturn>();
    public DbSet<SaleReturnLine> SaleReturnLines => Set<SaleReturnLine>();

    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<CashSession> CashSessions => Set<CashSession>();
    public DbSet<Expense> Expenses => Set<Expense>();

    public DbSet<License> Licenses => Set<License>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => new { u.CompanyID, u.Username })
            .IsUnique();

        modelBuilder.Entity<Permission>()
            .HasIndex(p => p.Code)
            .IsUnique();

        modelBuilder.Entity<RolePermission>()
            .HasKey(rp => new { rp.RoleID, rp.PermissionID });

        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleID);

        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionID);

        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserID, ur.RoleID });

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserID);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleID);

        // --- Products / Categories / Units ---

        modelBuilder.Entity<Category>()
            .HasOne(c => c.ParentCategory)
            .WithMany()
            .HasForeignKey(c => c.ParentCategoryID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Product>()
            .HasIndex(p => new { p.CompanyID, p.SKU })
            .IsUnique();

        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryID)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Product>()
            .HasOne(p => p.Unit)
            .WithMany(u => u.Products)
            .HasForeignKey(p => p.UnitID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProductBarcode>()
            .HasIndex(b => b.Code)
            .IsUnique();

        modelBuilder.Entity<ProductBarcode>()
            .HasOne(b => b.Product)
            .WithMany(p => p.Barcodes)
            .HasForeignKey(b => b.ProductID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProductPrice>()
            .HasIndex(pp => new { pp.ProductID, pp.PriceListID })
            .IsUnique();

        modelBuilder.Entity<ProductPrice>()
            .HasOne(pp => pp.Product)
            .WithMany(p => p.Prices)
            .HasForeignKey(pp => pp.ProductID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProductPrice>()
            .HasOne(pp => pp.PriceList)
            .WithMany(pl => pl.ProductPrices)
            .HasForeignKey(pp => pp.PriceListID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Product>()
            .Property(p => p.PurchasePrice).HasPrecision(18, 2);
        modelBuilder.Entity<Product>()
            .Property(p => p.CostPrice).HasPrecision(18, 2);
        modelBuilder.Entity<Product>()
            .Property(p => p.Price).HasPrecision(18, 2);
        modelBuilder.Entity<Product>()
            .Property(p => p.TaxRate).HasPrecision(5, 2);
        modelBuilder.Entity<ProductPrice>()
            .Property(pp => pp.Price).HasPrecision(18, 2);

        // --- Customers / Suppliers ---

        modelBuilder.Entity<Customer>()
            .Property(c => c.OpeningBalance).HasPrecision(18, 2);

        modelBuilder.Entity<CustomerTransaction>()
            .HasOne(t => t.Customer)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.CustomerID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CustomerTransaction>()
            .Property(t => t.Amount).HasPrecision(18, 2);

        modelBuilder.Entity<Supplier>()
            .Property(s => s.OpeningBalance).HasPrecision(18, 2);

        modelBuilder.Entity<SupplierTransaction>()
            .HasOne(t => t.Supplier)
            .WithMany(s => s.Transactions)
            .HasForeignKey(t => t.SupplierID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SupplierTransaction>()
            .Property(t => t.Amount).HasPrecision(18, 2);

        // --- Stock ---

        modelBuilder.Entity<StockTransaction>()
            .Property(t => t.Quantity).HasPrecision(18, 3);

        modelBuilder.Entity<StockTransaction>()
            .HasOne(t => t.Product)
            .WithMany()
            .HasForeignKey(t => t.ProductID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StockBalance>()
            .HasIndex(b => new { b.ProductID, b.WarehouseID })
            .IsUnique();

        modelBuilder.Entity<StockBalance>()
            .Property(b => b.CurrentQuantity).HasPrecision(18, 3);

        // --- Purchases ---

        modelBuilder.Entity<PurchaseOrderLine>()
            .HasOne(l => l.PurchaseOrder)
            .WithMany(o => o.Lines)
            .HasForeignKey(l => l.PurchaseOrderID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PurchaseOrderLine>()
            .Property(l => l.OrderedQty).HasPrecision(18, 3);
        modelBuilder.Entity<PurchaseOrderLine>()
            .Property(l => l.ReceivedQty).HasPrecision(18, 3);
        modelBuilder.Entity<PurchaseOrderLine>()
            .Property(l => l.UnitPrice).HasPrecision(18, 2);

        modelBuilder.Entity<GoodsReceiptLine>()
            .HasOne(l => l.GoodsReceipt)
            .WithMany(r => r.Lines)
            .HasForeignKey(l => l.GoodsReceiptID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GoodsReceiptLine>()
            .Property(l => l.ReceivedQty).HasPrecision(18, 3);

        modelBuilder.Entity<PurchaseInvoiceLine>()
            .HasOne(l => l.PurchaseInvoice)
            .WithMany(i => i.Lines)
            .HasForeignKey(l => l.PurchaseInvoiceID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PurchaseInvoiceLine>()
            .Property(l => l.Qty).HasPrecision(18, 3);
        modelBuilder.Entity<PurchaseInvoiceLine>()
            .Property(l => l.UnitPrice).HasPrecision(18, 2);
        modelBuilder.Entity<PurchaseInvoice>()
            .Property(i => i.TotalAmount).HasPrecision(18, 2);
        modelBuilder.Entity<PurchaseInvoice>()
            .Property(i => i.PaidAmount).HasPrecision(18, 2);

        // --- Sales ---

        modelBuilder.Entity<QuotationLine>()
            .HasOne(l => l.Quotation).WithMany(q => q.Lines)
            .HasForeignKey(l => l.QuotationID).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<QuotationLine>().Property(l => l.Qty).HasPrecision(18, 3);
        modelBuilder.Entity<QuotationLine>().Property(l => l.UnitPrice).HasPrecision(18, 2);

        modelBuilder.Entity<SalesOrderLine>()
            .HasOne(l => l.SalesOrder).WithMany(o => o.Lines)
            .HasForeignKey(l => l.SalesOrderID).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<SalesOrderLine>().Property(l => l.Qty).HasPrecision(18, 3);
        modelBuilder.Entity<SalesOrderLine>().Property(l => l.UnitPrice).HasPrecision(18, 2);

        modelBuilder.Entity<DeliveryNoteLine>()
            .HasOne(l => l.DeliveryNote).WithMany(d => d.Lines)
            .HasForeignKey(l => l.DeliveryNoteID).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<DeliveryNoteLine>().Property(l => l.Qty).HasPrecision(18, 3);

        modelBuilder.Entity<SaleInvoiceLine>()
            .HasOne(l => l.SaleInvoice).WithMany(i => i.Lines)
            .HasForeignKey(l => l.SaleInvoiceID).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<SaleInvoiceLine>().Property(l => l.Qty).HasPrecision(18, 3);
        modelBuilder.Entity<SaleInvoiceLine>().Property(l => l.UnitPrice).HasPrecision(18, 2);
        modelBuilder.Entity<SaleInvoiceLine>().Property(l => l.DiscountAmount).HasPrecision(18, 2);
        modelBuilder.Entity<SaleInvoiceLine>().Property(l => l.TaxAmount).HasPrecision(18, 2);

        modelBuilder.Entity<SaleInvoice>().Property(i => i.TotalAmount).HasPrecision(18, 2);
        modelBuilder.Entity<SaleInvoice>().Property(i => i.DiscountAmount).HasPrecision(18, 2);
        modelBuilder.Entity<SaleInvoice>().Property(i => i.TaxAmount).HasPrecision(18, 2);
        modelBuilder.Entity<SaleInvoice>().Property(i => i.PaidAmount).HasPrecision(18, 2);

        modelBuilder.Entity<SaleReturnLine>()
            .HasOne(l => l.SaleReturn).WithMany(r => r.Lines)
            .HasForeignKey(l => l.SaleReturnID).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<SaleReturnLine>().Property(l => l.Qty).HasPrecision(18, 3);

        // --- Payments / Cash / Expenses ---

        modelBuilder.Entity<Payment>().Property(p => p.Amount).HasPrecision(18, 2);

        modelBuilder.Entity<CashSession>().Property(c => c.OpeningBalance).HasPrecision(18, 2);
        modelBuilder.Entity<CashSession>().Property(c => c.ExpectedCash).HasPrecision(18, 2);
        modelBuilder.Entity<CashSession>().Property(c => c.ActualCash).HasPrecision(18, 2);
        modelBuilder.Entity<CashSession>().Property(c => c.Difference).HasPrecision(18, 2);

        modelBuilder.Entity<Expense>().Property(e => e.Amount).HasPrecision(18, 2);
    }
}
