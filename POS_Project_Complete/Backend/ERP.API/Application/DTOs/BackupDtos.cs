namespace ERP.API.Application.DTOs;

public record BackupResultDto(string FileName, DateTime CreatedAt, long SizeBytes);
public record BackupHistoryDto(string FileName, DateTime CreatedAt, long SizeBytes);
public record RestoreRequestDto(string FileName);
