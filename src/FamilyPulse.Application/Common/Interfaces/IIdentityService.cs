using FamilyPulse.Application.Identity.Commands;
using FamilyPulse.Application.Identity.Queries;

namespace FamilyPulse.Application.Common.Interfaces;

public interface IIdentityService
{
    /// <summary>
    /// Registers a new anonymous family account with either a custom or generated 4-word passphrase.
    /// </summary>
    Task<RegisterAnonymousFamilyResponse> RegisterAsync(
        RegisterAnonymousFamilyCommand command, 
        CancellationToken ct = default);

    /// <summary>
    /// Recovers an account using either Passphrase + Landmark OR a downloadable House Key JSON payload.
    /// </summary>
    Task<RecoverAccountResponse?> RecoverAsync(
        RecoverAccountQuery query, 
        CancellationToken ct = default);
}