namespace FamilyPulse.Application.Members.DTOs;

public record MemberDto(
    Guid Id, 
    Guid FamilyAccountId, 
    string Name, 
    string Role, 
    DateTime CreatedAtUtc
);

public record AddMemberCommand(
    string Name, 
    string Role
);

public record UpdateMemberCommand(
    string Name, 
    string Role
);