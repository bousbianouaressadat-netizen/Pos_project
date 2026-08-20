namespace ERP.API.Application.DTOs;

public record ActivateLicenseDto(string LicenseKey, string DeviceFingerprint);
public record LicenseStatusDto(bool IsActive, DateTime? ActivatedAt, string Status);
public record RevokeDeviceDto(string DeviceFingerprint);
