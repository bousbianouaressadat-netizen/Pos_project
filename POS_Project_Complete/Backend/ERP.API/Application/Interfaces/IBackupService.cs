using ERP.API.Application.DTOs;

namespace ERP.API.Application.Interfaces;

public interface IBackupService
{
    Task<BackupResultDto> CreateBackupAsync(Guid companyId, Guid userId);
    Task<List<BackupHistoryDto>> GetHistoryAsync(Guid companyId);
    Task RestoreAsync(Guid companyId, string backupFileName, Guid userId);
}
