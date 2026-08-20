using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using ERP.API.Domain.Entities;
using ERP.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Application.Services;

public class SalesService : ISalesService
{
    private readonly AppDbContext _db;
    private readonly IStockService _stockService;

    public SalesService(AppDbContext db, IStockService stockService)
    {
        _db = db;
        _stockService = stockService;
    }

    public async Task<Guid> CreateSaleInvoiceAsync(Guid companyId, CreateSaleInvoiceDto dto, Guid userId)
    {
        if (dto.Lines.Count == 0)
            throw new InvalidOperationException("الفاتورة يجب أن تحتوي سطرًا واحدًا على الأقل");

        var products = await _db.Products
            .Where(p => p.CompanyID == companyId && dto.Lines.Select(l => l.ProductID).Contains(p.ProductID))
            .ToDictionaryAsync(p => p.ProductID);

        var invoice = new SaleInvoice
        {
            CompanyID = companyId,
            CustomerID = dto.CustomerID,
            UserID = userId,
            WarehouseID = dto.WarehouseID,
            DiscountAmount = dto.InvoiceDiscountAmount,
            PaidAmount = dto.PaidAmount,
            PaymentMethod = dto.PaymentMethod,
            Status = dto.Status == "Held" ? "Held" : "Completed"
        };

        decimal subTotal = 0, totalTax = 0;

        foreach (var line in dto.Lines)
        {
            if (!products.TryGetValue(line.ProductID, out var product))
                throw new InvalidOperationException("منتج غير موجود بالفاتورة");

            var lineTax = (line.Qty * line.UnitPrice - line.DiscountAmount) * (product.TaxRate / 100m);

            invoice.Lines.Add(new SaleInvoiceLine
            {
                ProductID = line.ProductID,
                Qty = line.Qty,
                UnitPrice = line.UnitPrice,
                DiscountAmount = line.DiscountAmount,
                TaxAmount = lineTax,
                SaleInvoice = invoice
            });

            subTotal += line.Qty * line.UnitPrice;
            totalTax += lineTax;
        }

        invoice.TaxAmount = totalTax;
        invoice.TotalAmount = subTotal - dto.InvoiceDiscountAmount + totalTax;

        _db.SaleInvoices.Add(invoice);
        await _db.SaveChangesAsync();

        // الفاتورة "المعلّقة" (Held) لا تؤثر على المخزون ولا الديون بعد — فقط عند التأكيد النهائي
        if (invoice.Status == "Completed")
        {
            // خصم المخزون هنا مباشرة (القرار المُتَّخذ بالمرحلة الأولى)
            foreach (var line in dto.Lines)
            {
                await _stockService.RecordMovementAsync(
                    line.ProductID, dto.WarehouseID, -line.Qty,
                    "Sale", nameof(SaleInvoice), invoice.SaleInvoiceID, userId);
            }

            // لو فيه عميل مسجَّل، سجّل الدين بالـ Ledger (فرق ما بين الإجمالي والمدفوع)
            if (dto.CustomerID.HasValue)
            {
                _db.CustomerTransactions.Add(new CustomerTransaction
                {
                    CustomerID = dto.CustomerID.Value,
                    Type = "Invoice",
                    Amount = invoice.TotalAmount,
                    ReferenceType = nameof(SaleInvoice),
                    ReferenceID = invoice.SaleInvoiceID
                });

                if (dto.PaidAmount > 0)
                {
                    _db.CustomerTransactions.Add(new CustomerTransaction
                    {
                        CustomerID = dto.CustomerID.Value,
                        Type = "Payment",
                        Amount = -dto.PaidAmount,
                        ReferenceType = nameof(SaleInvoice),
                        ReferenceID = invoice.SaleInvoiceID
                    });
                }

                await _db.SaveChangesAsync();
            }
        }

        return invoice.SaleInvoiceID;
    }

    public async Task<List<SaleInvoiceListItemDto>> GetInvoicesAsync(Guid companyId)
    {
        var customers = await _db.Customers.ToDictionaryAsync(c => c.CustomerID, c => c.Name);

        return await _db.SaleInvoices
            .Where(i => i.CompanyID == companyId)
            .Select(i => new SaleInvoiceListItemDto(
                i.SaleInvoiceID,
                i.CustomerID != null ? customers.GetValueOrDefault(i.CustomerID.Value) : null,
                i.TotalAmount, i.PaidAmount, i.Status, i.CreatedAt))
            .ToListAsync();
    }

    public async Task<SaleInvoiceDto?> GetInvoiceByIdAsync(Guid companyId, Guid invoiceId)
    {
        var invoice = await _db.SaleInvoices
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.CompanyID == companyId && i.SaleInvoiceID == invoiceId);

        if (invoice is null) return null;

        var productNames = await _db.Products.ToDictionaryAsync(p => p.ProductID, p => p.NameAR);
        string? customerName = invoice.CustomerID.HasValue
            ? (await _db.Customers.FindAsync(invoice.CustomerID.Value))?.Name
            : null;

        return new SaleInvoiceDto(
            invoice.SaleInvoiceID, invoice.CustomerID, customerName,
            invoice.TotalAmount, invoice.DiscountAmount, invoice.TaxAmount, invoice.PaidAmount,
            invoice.Status, invoice.PaymentMethod, invoice.CreatedAt,
            invoice.Lines.Select(l => new SaleInvoiceLineDto(
                l.ProductID, productNames.GetValueOrDefault(l.ProductID, ""),
                l.Qty, l.UnitPrice, l.DiscountAmount, l.TaxAmount)).ToList()
        );
    }

    /// <summary>
    /// حذف فاتورة بيع: عملية حساسة تتطلب صلاحية CanDeleteSale (يُتحقَّق منها بالـ Controller)،
    /// وتُرجع المخزون وتُلغي دين العميل المرتبط بها.
    /// </summary>
    public async Task<bool> DeleteInvoiceAsync(Guid companyId, Guid invoiceId, Guid userId)
    {
        var invoice = await _db.SaleInvoices
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.CompanyID == companyId && i.SaleInvoiceID == invoiceId);

        if (invoice is null) return false;

        if (invoice.Status == "Completed")
        {
            foreach (var line in invoice.Lines)
            {
                await _stockService.RecordMovementAsync(
                    line.ProductID, invoice.WarehouseID, line.Qty,
                    "SalesReturn", nameof(SaleInvoice), invoice.SaleInvoiceID, userId);
            }

            if (invoice.CustomerID.HasValue)
            {
                _db.CustomerTransactions.Add(new CustomerTransaction
                {
                    CustomerID = invoice.CustomerID.Value,
                    Type = "Return",
                    Amount = -invoice.TotalAmount,
                    ReferenceType = nameof(SaleInvoice),
                    ReferenceID = invoice.SaleInvoiceID
                });
            }
        }

        _db.AuditLogs.Add(new AuditLog
        {
            UserID = userId,
            EntityName = nameof(SaleInvoice),
            EntityID = invoiceId.ToString(),
            Action = "Delete",
            OldValue = $"TotalAmount={invoice.TotalAmount}"
        });

        invoice.Status = "Returned";
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<Guid> CreateReturnAsync(Guid companyId, Guid invoiceId, CreateSaleReturnDto dto, Guid userId)
    {
        var invoice = await _db.SaleInvoices
            .FirstOrDefaultAsync(i => i.CompanyID == companyId && i.SaleInvoiceID == invoiceId);

        if (invoice is null) throw new InvalidOperationException("الفاتورة غير موجودة");

        var saleReturn = new SaleReturn { CompanyID = companyId, SaleInvoiceID = invoiceId, UserID = userId };
        decimal returnValue = 0;

        var priceByProduct = (await _db.SaleInvoiceLines
            .Where(l => l.SaleInvoiceID == invoiceId)
            .ToListAsync())
            .ToDictionary(l => l.ProductID, l => l.UnitPrice);

        foreach (var line in dto.Lines)
        {
            saleReturn.Lines.Add(new SaleReturnLine
            {
                ProductID = line.ProductID,
                Qty = line.Qty,
                Reason = line.Reason,
                SaleReturn = saleReturn
            });

            await _stockService.RecordMovementAsync(
                line.ProductID, invoice.WarehouseID, line.Qty,
                "SalesReturn", nameof(SaleReturn), saleReturn.SaleReturnID, userId);

            if (priceByProduct.TryGetValue(line.ProductID, out var price))
                returnValue += line.Qty * price;
        }

        _db.SaleReturns.Add(saleReturn);

        if (invoice.CustomerID.HasValue && returnValue > 0)
        {
            _db.CustomerTransactions.Add(new CustomerTransaction
            {
                CustomerID = invoice.CustomerID.Value,
                Type = "Return",
                Amount = -returnValue,
                ReferenceType = nameof(SaleReturn),
                ReferenceID = saleReturn.SaleReturnID
            });
        }

        await _db.SaveChangesAsync();
        return saleReturn.SaleReturnID;
    }

    // --- وثائق مترابطة: Quotation → SalesOrder → DeliveryNote ---

    public async Task<Guid> CreateQuotationAsync(Guid companyId, CreateQuotationDto dto)
    {
        var quotation = new Quotation { CompanyID = companyId, CustomerID = dto.CustomerID };
        foreach (var line in dto.Lines)
            quotation.Lines.Add(new QuotationLine { ProductID = line.ProductID, Qty = line.Qty, UnitPrice = line.UnitPrice, Quotation = quotation });

        _db.Quotations.Add(quotation);
        await _db.SaveChangesAsync();
        return quotation.QuotationID;
    }

    public async Task<Guid> CreateSalesOrderFromQuotationAsync(Guid companyId, CreateSalesOrderFromQuotationDto dto)
    {
        var quotation = await _db.Quotations
            .Include(q => q.Lines)
            .FirstOrDefaultAsync(q => q.CompanyID == companyId && q.QuotationID == dto.QuotationID);

        if (quotation is null) throw new InvalidOperationException("عرض السعر غير موجود");

        var order = new SalesOrder
        {
            CompanyID = companyId,
            CustomerID = quotation.CustomerID,
            ReferenceQuotationID = quotation.QuotationID
        };

        foreach (var line in quotation.Lines)
            order.Lines.Add(new SalesOrderLine { ProductID = line.ProductID, Qty = line.Qty, UnitPrice = line.UnitPrice, SalesOrder = order });

        quotation.Status = "Confirmed";

        _db.SalesOrders.Add(order);
        await _db.SaveChangesAsync();
        return order.SalesOrderID;
    }

    public async Task<Guid> CreateDeliveryNoteFromOrderAsync(Guid companyId, CreateDeliveryNoteFromOrderDto dto)
    {
        var order = await _db.SalesOrders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.CompanyID == companyId && o.SalesOrderID == dto.SalesOrderID);

        if (order is null) throw new InvalidOperationException("أمر البيع غير موجود");

        var note = new DeliveryNote { CompanyID = companyId, ReferenceSalesOrderID = order.SalesOrderID };
        foreach (var line in order.Lines)
            note.Lines.Add(new DeliveryNoteLine { ProductID = line.ProductID, Qty = line.Qty, DeliveryNote = note });

        order.Status = "Delivered";

        _db.DeliveryNotes.Add(note);
        await _db.SaveChangesAsync();
        return note.DeliveryNoteID;
    }
}
