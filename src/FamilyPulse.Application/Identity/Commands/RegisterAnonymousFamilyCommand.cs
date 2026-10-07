namespace FamilyPulse.Application.Identity.Commands;

public record CreateInitialMemberDto(string Name, string Role);

public record RegisterAnonymousFamilyResponse(
    Guid FamilyId,
    string Passphrase,
    string VirtualLandmarkId,
    string HouseKeyJson,
    DateTime CreatedAtUtc
);

// Plain positional record — no custom interface required!
public record RegisterAnonymousFamilyCommand(
    string VirtualLandmarkId,
    string? CustomPassphrase = null,
    List<CreateInitialMemberDto>? InitialMembers = null
);