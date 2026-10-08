using FamilyPulse.Application.Common.Interfaces;
using FamilyPulse.Application.Identity.Commands;
using FamilyPulse.Application.Identity.Queries;
using FamilyPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyPulse.Application.Identity.Services;

public class IdentityService : IIdentityService
{
    private readonly IAppDbContext _dbContext;
    private readonly IIdentityHasher _identityHasher;
    private readonly IHouseKeyService _houseKeyService;
    private readonly IPassphraseGenerator _passphraseGenerator;

    public IdentityService(
        IAppDbContext dbContext,
        IIdentityHasher identityHasher,
        IHouseKeyService houseKeyService,
        IPassphraseGenerator passphraseGenerator)
    {
        _dbContext = dbContext;
        _identityHasher = identityHasher;
        _houseKeyService = houseKeyService;
        _passphraseGenerator = passphraseGenerator;
    }

    public async Task<RegisterAnonymousFamilyResponse> RegisterAsync(
        RegisterAnonymousFamilyCommand command, 
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.VirtualLandmarkId))
        {
            throw new ArgumentException("Virtual landmark ID is required.", nameof(command.VirtualLandmarkId));
        }

        string finalPassphrase;
        string identityHash;
        bool isCustom = !string.IsNullOrWhiteSpace(command.CustomPassphrase);

        if (isCustom)
        {
            finalPassphrase = command.CustomPassphrase!.Trim().ToLowerInvariant();
            identityHash = _identityHasher.HashIdentity(finalPassphrase, command.VirtualLandmarkId);

            bool exists = await _dbContext.FamilyAccounts
                .AnyAsync(x => x.IdentityHash == identityHash, ct);

            if (exists)
            {
                throw new InvalidOperationException(
                    "This 4-word passphrase is already taken in this landmark area. Please select another set of words.");
            }
        }
        else
        {
            // Auto-generate unique 4-word passphrase with retry loop
            const int maxRetries = 5;
            int attempts = 0;

            do
            {
                finalPassphrase = _passphraseGenerator.Generate4WordPassphrase();
                identityHash = _identityHasher.HashIdentity(finalPassphrase, command.VirtualLandmarkId);
                
                bool exists = await _dbContext.FamilyAccounts
                    .AnyAsync(x => x.IdentityHash == identityHash, ct);

                if (!exists) break;
                attempts++;
            } 
            while (attempts < maxRetries);

            if (attempts >= maxRetries)
            {
                throw new InvalidOperationException("Could not generate a unique passphrase. Please try again.");
            }
        }

        // 1. Create domain entity
        var account = new FamilyAccount(identityHash);

        // 2. Attach initial family members if provided
        if (command.InitialMembers is { Count: > 0 })
        {
            foreach (var member in command.InitialMembers)
            {
                account.AddMember(member.Name, member.Role);
            }
        }

        // 3. Persist to database
        _dbContext.FamilyAccounts.Add(account);
        await _dbContext.SaveChangesAsync(ct);

        // 4. Generate downloadable House Key JSON
        string houseKeyJson = _houseKeyService.ExportHouseKeyJson(account.Id);

        return new RegisterAnonymousFamilyResponse(
            FamilyId: account.Id,
            Passphrase: finalPassphrase,
            VirtualLandmarkId: command.VirtualLandmarkId,
            HouseKeyJson: houseKeyJson,
            CreatedAtUtc: account.CreatedAtUtc
        );
    }



public async Task<RecoverAccountResponse?> RecoverAsync(
    RecoverAccountQuery query, 
    CancellationToken ct = default)
{
    FamilyAccount? account = null;

    // Diagnostic log: Check what C# actually received from the JSON payload
    Console.WriteLine($"[RECOVER ATTEMPT] Passphrase: '{query.Passphrase}', Landmark: '{query.VirtualLandmarkId}', HasKeyJson: {!string.IsNullOrWhiteSpace(query.HouseKeyJson)}");

    // Path A: Recovery via imported House Key JSON file
    if (!string.IsNullOrWhiteSpace(query.HouseKeyJson))
    {
        bool isValidKey = _houseKeyService.TryValidateHouseKey(query.HouseKeyJson, out Guid familyId);
        Console.WriteLine($"[RECOVER KEY] Validation Result: {isValidKey}, FamilyId: {familyId}");

        if (isValidKey)
        {
            account = await _dbContext.FamilyAccounts
                .FirstOrDefaultAsync(x => x.Id == familyId, ct);
            Console.WriteLine($"[RECOVER KEY] Account found in DB: {account is not null}");
        }
    }
    // Path B: Recovery via Passphrase + Landmark
    else if (!string.IsNullOrWhiteSpace(query.Passphrase) && !string.IsNullOrWhiteSpace(query.VirtualLandmarkId))
    {
        string targetHash = _identityHasher.HashIdentity(query.Passphrase, query.VirtualLandmarkId);
        Console.WriteLine($"[RECOVER HASH] Computed Hash: {targetHash}");

        account = await _dbContext.FamilyAccounts
            .FirstOrDefaultAsync(x => x.IdentityHash == targetHash, ct);
        Console.WriteLine($"[RECOVER HASH] Account found in DB: {account is not null}");
    }

    if (account is null)
    {
        Console.WriteLine("[RECOVER FAILED] Returning null (HTTP 404)");
        return null;
    }

    account.RecordActivity();
    await _dbContext.SaveChangesAsync(ct);

    string houseKeyJson = _houseKeyService.ExportHouseKeyJson(account.Id);

    return new RecoverAccountResponse(
        FamilyId: account.Id,
        HouseKeyJson: houseKeyJson,
        CreatedAtUtc: account.CreatedAtUtc,
        LastActiveAtUtc: account.LastActiveAtUtc
    );
}




}