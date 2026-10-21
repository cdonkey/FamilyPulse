using FamilyPulse.Domain.Entities;
namespace FamilyPulse.Application.CheckIns.Dtos;

public record CheckInDto(
    Guid Id,
    Guid FamilyAccountId,
    Guid MemberId,
    CheckInType Type,
    int StressLevel,
    int ClosenessLevel,
    int EnergyLevel,
    string? Note,
    List<string> Tags,
    DateTime CreatedAtUtc
);

public record CreateCheckInCommand(
    Guid MemberId,
    CheckInType Type,
    int StressLevel,
    int ClosenessLevel,
    int EnergyLevel,
    string? Note,
    List<string>? Tags
);