using ERP.API.Application.DTOs;

namespace ERP.API.Application.Interfaces;

public interface ILicenseService
{
    Task ActivateAsync(Guid companyId, ActivateLicenseDto dto);
    Task<LicenseStatusDto> GetStatusAsync(Guid companyId, string deviceFingerprint);
    Task RevokeDeviceAsync(Guid companyId, RevokeDeviceDto dto);
}
