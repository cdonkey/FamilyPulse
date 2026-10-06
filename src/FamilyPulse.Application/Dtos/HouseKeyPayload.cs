namespace FamilyPulse.Application.Common.Models;

public record HouseKeyPayload(
    Guid FamilyId,
    int Version,
    DateTime IssuedAtUtc,
    string Signature
);