using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using ERP.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Application.Services;

public class PrintingService : IPrintingService
{
    private readonly AppDbContext _db;
    public PrintingService(AppDbContext db) => _db = db;

    public async Task<List<BarcodeLabelDto>> GetLabelsAsync(Guid companyId, List<Guid> productIds)
    {
        var products = await _db.Products
            .Where(p => p.CompanyID == companyId && productIds.Contains(p.ProductID))
            .Include(p => p.Barcodes)
            .ToListAsync();

        return products.Select(p => new BarcodeLabelDto(
            p.SKU, p.NameAR, p.NameFR,
            p.Barcodes.FirstOrDefault()?.Code ?? p.SKU, // لو ما عنده باركود، الملصق يستخدم SKU كباركود داخلي
            p.Price
        )).ToList();
    }

    public async Task<InvoicePrintDataDto?> GetInvoicePrintDataAsync(Guid companyId, Guid invoiceId)
    {
        var invoice = await _db.SaleInvoices
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.CompanyID == companyId && i.SaleInvoiceID == invoiceId);

        if (invoice is null) return null;

        var company = await _db.Companies.FirstOrDefaultAsync(c => c.CompanyID == companyId);
        var productNames = await _db.Products.ToDictionaryAsync(p => p.ProductID, p => p.NameAR);
        string? customerName = invoice.CustomerID.HasValue
            ? (await _db.Customers.FindAsync(invoice.CustomerID.Value))?.Name
            : null;

        var lines = invoice.Lines.Select(l => new InvoicePrintLineDto(
            productNames.GetValueOrDefault(l.ProductID, ""), l.Qty, l.UnitPrice,
            l.Qty * l.UnitPrice - l.DiscountAmount + l.TaxAmount
        )).ToList();

        return new InvoicePrintDataDto(
            invoice.SaleInvoiceID, company?.Name ?? "", company?.NIF, company?.RC,
            customerName, invoice.CreatedAt, lines,
            invoice.TotalAmount, invoice.DiscountAmount, invoice.TaxAmount, invoice.PaidAmount,
            invoice.TotalAmount - invoice.PaidAmount
        );
    }
}
