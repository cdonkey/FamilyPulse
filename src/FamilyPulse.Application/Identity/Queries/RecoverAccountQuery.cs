namespace FamilyPulse.Application.Identity.Queries;

public record RecoverAccountQuery(
    string? Passphrase = null,
    string? VirtualLandmarkId = null,
    string? HouseKeyJson = null
);

public record RecoverAccountResponse(
    Guid FamilyId,
    string HouseKeyJson,
    DateTime CreatedAtUtc,
    DateTime LastActiveAtUtc
);