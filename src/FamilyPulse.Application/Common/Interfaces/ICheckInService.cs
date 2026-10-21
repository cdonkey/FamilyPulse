using  FamilyPulse.Application.CheckIns.Dtos;
namespace FamilyPulse.Application.Common.Interfaces;

public interface ICheckInService
{
    Task<CheckInDto?> CreateCheckInAsync(Guid familyId, CreateCheckInCommand command, CancellationToken ct = default);
    Task<List<CheckInDto>> GetCheckInsForFamilyAsync(Guid familyId, int limit = 20, CancellationToken ct = default);
}