using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using ERP.API.Domain.Entities;
using ERP.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Application.Services;

public class PurchaseService : IPurchaseService
{
    private readonly AppDbContext _db;
    private readonly IStockService _stockService;

    public PurchaseService(AppDbContext db, IStockService stockService)
    {
        _db = db;
        _stockService = stockService;
    }

    public async Task<Guid> CreatePurchaseOrderAsync(Guid companyId, CreatePurchaseOrderDto dto)
    {
        var order = new PurchaseOrder { CompanyID = companyId, SupplierID = dto.SupplierID, Status = "Open" };

        foreach (var line in dto.Lines)
        {
            order.Lines.Add(new PurchaseOrderLine
            {
                ProductID = line.ProductID,
                OrderedQty = line.OrderedQty,
                UnitPrice = line.UnitPrice,
                ReceivedQty = 0,
                PurchaseOrder = order
            });
        }

        _db.PurchaseOrders.Add(order);
        await _db.SaveChangesAsync();
        return order.PurchaseOrderID;
    }

    public async Task<List<PurchaseOrderDto>> GetPurchaseOrdersAsync(Guid companyId)
    {
        var orders = await _db.PurchaseOrders
            .Where(o => o.CompanyID == companyId)
            .Include(o => o.Supplier)
            .Include(o => o.Lines).ThenInclude(l => l.PurchaseOrder)
            .ToListAsync();

        var productNames = await _db.Products.ToDictionaryAsync(p => p.ProductID, p => p.NameAR);

        return orders.Select(o => new PurchaseOrderDto(
            o.PurchaseOrderID, o.SupplierID, o.Supplier!.Name, o.Status, o.CreatedAt,
            o.Lines.Select(l => new PurchaseOrderLineDto(
                l.ProductID, productNames.GetValueOrDefault(l.ProductID, ""),
                l.OrderedQty, l.ReceivedQty, l.UnitPrice)).ToList()
        )).ToList();
    }

    public async Task<PurchaseOrderDto?> GetPurchaseOrderByIdAsync(Guid companyId, Guid id)
    {
        var order = await _db.PurchaseOrders
            .Include(o => o.Supplier)
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.CompanyID == companyId && o.PurchaseOrderID == id);

        if (order is null) return null;

        var productNames = await _db.Products.ToDictionaryAsync(p => p.ProductID, p => p.NameAR);

        return new PurchaseOrderDto(
            order.PurchaseOrderID, order.SupplierID, order.Supplier!.Name, order.Status, order.CreatedAt,
            order.Lines.Select(l => new PurchaseOrderLineDto(
                l.ProductID, productNames.GetValueOrDefault(l.ProductID, ""),
                l.OrderedQty, l.ReceivedQty, l.UnitPrice)).ToList()
        );
    }

    /// <summary>
    /// المخزون يزيد فقط هنا (Business Rule رقم 2) — وليس عند إنشاء PurchaseOrder.
    /// </summary>
    public async Task<Guid> CreateGoodsReceiptAsync(Guid companyId, CreateGoodsReceiptDto dto, Guid userId)
    {
        var order = await _db.PurchaseOrders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.CompanyID == companyId && o.PurchaseOrderID == dto.PurchaseOrderID);

        if (order is null) throw new InvalidOperationException("أمر الشراء غير موجود");

        var receipt = new GoodsReceipt
        {
            CompanyID = companyId,
            PurchaseOrderID = dto.PurchaseOrderID,
            WarehouseID = dto.WarehouseID,
            UserID = userId
        };

        foreach (var line in dto.Lines)
        {
            var orderLine = order.Lines.FirstOrDefault(l => l.ProductID == line.ProductID);
            if (orderLine is null)
                throw new InvalidOperationException("المنتج غير موجود بأمر الشراء الأصلي");

            var remaining = orderLine.OrderedQty - orderLine.ReceivedQty;
            if (line.ReceivedQty > remaining)
                throw new InvalidOperationException($"الكمية المستلمة تتجاوز المتبقي بأمر الشراء (المتبقي: {remaining})");

            orderLine.ReceivedQty += line.ReceivedQty;

            receipt.Lines.Add(new GoodsReceiptLine
            {
                ProductID = line.ProductID,
                ReceivedQty = line.ReceivedQty,
                GoodsReceipt = receipt
            });
        }

        // تحديث حالة أمر الشراء
        order.Status = order.Lines.All(l => l.ReceivedQty >= l.OrderedQty) ? "Received" : "PartiallyReceived";

        _db.GoodsReceipts.Add(receipt);
        await _db.SaveChangesAsync();

        // زيادة المخزون فعليًا لكل سطر مستلم
        foreach (var line in dto.Lines)
        {
            await _stockService.RecordMovementAsync(
                line.ProductID, dto.WarehouseID, line.ReceivedQty,
                "Purchase", nameof(GoodsReceipt), receipt.GoodsReceiptID, userId);
        }

        return receipt.GoodsReceiptID;
    }

    public async Task<Guid> CreatePurchaseInvoiceAsync(Guid companyId, CreatePurchaseInvoiceDto dto, Guid userId)
    {
        var total = dto.Lines.Sum(l => l.Qty * l.UnitPrice);

        var invoice = new PurchaseInvoice
        {
            CompanyID = companyId,
            SupplierID = dto.SupplierID,
            GoodsReceiptID = dto.GoodsReceiptID,
            TotalAmount = total,
            PaidAmount = dto.PaidAmount,
            Status = dto.PaidAmount >= total ? "Paid" : (dto.PaidAmount > 0 ? "PartiallyPaid" : "Unpaid"),
            UserID = userId
        };

        foreach (var line in dto.Lines)
        {
            invoice.Lines.Add(new PurchaseInvoiceLine
            {
                ProductID = line.ProductID,
                Qty = line.Qty,
                UnitPrice = line.UnitPrice,
                PurchaseInvoice = invoice
            });
        }

        _db.PurchaseInvoices.Add(invoice);

        // Ledger المورد: دين علينا بكامل قيمة الفاتورة
        _db.SupplierTransactions.Add(new SupplierTransaction
        {
            SupplierID = dto.SupplierID,
            Type = "PurchaseInvoice",
            Amount = total,
            ReferenceType = nameof(PurchaseInvoice),
            ReferenceID = invoice.PurchaseInvoiceID
        });

        // لو دُفع جزء أو كامل فورًا، يُسجَّل كدفعة تخفّض الدين مباشرة
        if (dto.PaidAmount > 0)
        {
            _db.SupplierTransactions.Add(new SupplierTransaction
            {
                SupplierID = dto.SupplierID,
                Type = "Payment",
                Amount = -dto.PaidAmount,
                ReferenceType = nameof(PurchaseInvoice),
                ReferenceID = invoice.PurchaseInvoiceID
            });
        }

        await _db.SaveChangesAsync();
        return invoice.PurchaseInvoiceID;
    }

    public async Task<List<PurchaseInvoiceDto>> GetPurchaseInvoicesAsync(Guid companyId)
    {
        return await _db.PurchaseInvoices
            .Where(i => i.CompanyID == companyId)
            .Include(i => i.Lines)
            .Select(i => new PurchaseInvoiceDto(
                i.PurchaseInvoiceID, i.SupplierID,
                _db.Suppliers.Where(s => s.SupplierID == i.SupplierID).Select(s => s.Name).FirstOrDefault() ?? "",
                i.TotalAmount, i.PaidAmount, i.Status, i.CreatedAt))
            .ToListAsync();
    }
}
