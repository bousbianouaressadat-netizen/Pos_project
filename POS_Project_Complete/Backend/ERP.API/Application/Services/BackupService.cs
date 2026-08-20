using System.Diagnostics;
using ERP.API.Application.DTOs;
using ERP.API.Application.Interfaces;
using ERP.API.Domain.Entities;
using ERP.API.Infrastructure.Data;

namespace ERP.API.Application.Services;

public class BackupService : IBackupService
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _db;

    public BackupService(IConfiguration config, AppDbContext db)
    {
        _config = config;
        _db = db;
    }

    public async Task<BackupResultDto> CreateBackupAsync(Guid companyId, Guid userId)
    {
        var section = _config.GetSection("Backup");
        var directory = section["Directory"]!;
        Directory.CreateDirectory(directory);

        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var fileName = $"backup_{timestamp}.dump";
        var fullPath = Path.Combine(directory, fileName);

        var psi = new ProcessStartInfo
        {
            FileName = section["PgDumpPath"],
            Arguments = $"-h {section["PostgresHost"]} -p {section["PostgresPort"]} " +
                        $"-U {section["PostgresUsername"]} -F c -f \"{fullPath}\" {section["PostgresDatabase"]}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        psi.Environment["PGPASSWORD"] = section["PostgresPassword"];

        using var process = Process.Start(psi)!;
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
            throw new InvalidOperationException($"فشل النسخ الاحتياطي: {error}");

        _db.AuditLogs.Add(new AuditLog
        {
            UserID = userId,
            EntityName = "Backup",
            EntityID = fileName,
            Action = "Create"
        });
        await _db.SaveChangesAsync();

        // حذف أقدم النسخ لو تجاوزنا عدد الاحتفاظ المسموح (RetainCount)
        await CleanupOldBackupsAsync(directory, int.Parse(section["RetainCount"] ?? "15"));

        var size = new FileInfo(fullPath).Length;
        return new BackupResultDto(fileName, DateTime.UtcNow, size);
    }

    public Task<List<BackupHistoryDto>> GetHistoryAsync(Guid companyId)
    {
        var directory = _config["Backup:Directory"]!;
        if (!Directory.Exists(directory))
            return Task.FromResult(new List<BackupHistoryDto>());

        var files = Directory.GetFiles(directory, "backup_*.dump")
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.CreationTimeUtc)
            .Select(f => new BackupHistoryDto(f.Name, f.CreationTimeUtc, f.Length))
            .ToList();

        return Task.FromResult(files);
    }

    public async Task RestoreAsync(Guid companyId, string backupFileName, Guid userId)
    {
        var section = _config.GetSection("Backup");
        var directory = section["Directory"]!;
        var fullPath = Path.Combine(directory, backupFileName);

        if (!File.Exists(fullPath))
            throw new InvalidOperationException("ملف النسخة الاحتياطية غير موجود");

        var psi = new ProcessStartInfo
        {
            FileName = section["PgRestorePath"],
            Arguments = $"-h {section["PostgresHost"]} -p {section["PostgresPort"]} " +
                        $"-U {section["PostgresUsername"]} -d {section["PostgresDatabase"]} --clean --if-exists \"{fullPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        psi.Environment["PGPASSWORD"] = section["PostgresPassword"];

        using var process = Process.Start(psi)!;
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
            throw new InvalidOperationException($"فشلت الاستعادة: {error}");

        _db.AuditLogs.Add(new AuditLog
        {
            UserID = userId,
            EntityName = "Backup",
            EntityID = backupFileName,
            Action = "Restore"
        });
        await _db.SaveChangesAsync();
    }

    private static Task CleanupOldBackupsAsync(string directory, int retainCount)
    {
        var files = Directory.GetFiles(directory, "backup_*.dump")
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.CreationTimeUtc)
            .Skip(retainCount);

        foreach (var file in files)
            file.Delete();

        return Task.CompletedTask;
    }
}
