using System.Security.Cryptography;
using System.Text;
using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using ERP.API.Domain.Entities;
using ERP.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Application.Services;

public class LicenseService : ILicenseService
{
    private readonly AppDbContext _db;
    public LicenseService(AppDbContext db) => _db = db;

    public async Task ActivateAsync(Guid companyId, ActivateLicenseDto dto)
    {
        var keyHash = HashKey(dto.LicenseKey);

        var existingForKey = await _db.Licenses
            .Where(l => l.LicenseKeyHash == keyHash && l.Status == "Active")
            .ToListAsync();

        // نفس الجهاز يعيد التفعيل بنفس المفتاح → عملية Idempotent، لا خطأ
        if (existingForKey.Any(l => l.DeviceFingerprint == dto.DeviceFingerprint))
            return;

        var maxDevices = existingForKey.FirstOrDefault()?.MaxDevices ?? 1;
        if (existingForKey.Count >= maxDevices)
            throw new InvalidOperationException("تم تفعيل هذا المفتاح على العدد الأقصى المسموح من الأجهزة");

        _db.Licenses.Add(new License
        {
            CompanyID = companyId,
            LicenseKeyHash = keyHash,
            DeviceFingerprint = dto.DeviceFingerprint,
            MaxDevices = maxDevices,
            Status = "Active"
        });

        await _db.SaveChangesAsync();
    }

    public async Task<LicenseStatusDto> GetStatusAsync(Guid companyId, string deviceFingerprint)
    {
        var license = await _db.Licenses
            .FirstOrDefaultAsync(l => l.CompanyID == companyId
                                       && l.DeviceFingerprint == deviceFingerprint
                                       && l.Status == "Active");

        return license is null
            ? new LicenseStatusDto(false, null, "NotActivated")
            : new LicenseStatusDto(true, license.ActivatedAt, license.Status);
    }

    public async Task RevokeDeviceAsync(Guid companyId, RevokeDeviceDto dto)
    {
        var license = await _db.Licenses
            .FirstOrDefaultAsync(l => l.CompanyID == companyId && l.DeviceFingerprint == dto.DeviceFingerprint);

        if (license is null) throw new InvalidOperationException("لا يوجد ترخيص لهذا الجهاز");

        license.Status = "Revoked";
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Hash بسيط (SHA256) وليس PBKDF2 — هنا الهدف عدم تخزين المفتاح كنص واضح مع إمكانية
    /// البحث عن نفس المفتاح لاحقًا (عكس كلمات السر، لا نحتاج Salt عشوائي هنا).
    /// </summary>
    private static string HashKey(string key)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return Convert.ToBase64String(bytes);
    }
}
