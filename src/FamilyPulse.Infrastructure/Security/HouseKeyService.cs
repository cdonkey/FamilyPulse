using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FamilyPulse.Application.Common.Interfaces;
using FamilyPulse.Application.Common.Models;
using Microsoft.Extensions.Configuration;

namespace FamilyPulse.Infrastructure.Security;

public class HouseKeyService : IHouseKeyService
{
    private readonly byte[] _signingKey;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public HouseKeyService(IConfiguration configuration)
    {
        var secret = configuration["Security:HouseKeySecret"] 
                     ?? "FamilyPulse_Default_HouseKey_Secret_ChangeInProduction_2026!";
        _signingKey = Encoding.UTF8.GetBytes(secret);
    }

    public string ExportHouseKeyJson(Guid familyId)
    {
        var issuedAt = DateTime.UtcNow;
        const int version = 1;

        var signature = ComputeSignature(familyId, version, issuedAt);

        var payload = new HouseKeyPayload(
            FamilyId: familyId,
            Version: version,
            IssuedAtUtc: issuedAt,
            Signature: signature
        );

        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    public bool TryValidateHouseKey(string jsonContent, out Guid familyId)
    {
        familyId = Guid.Empty;

        if (string.IsNullOrWhiteSpace(jsonContent))
            return false;

        try
        {
            var payload = JsonSerializer.Deserialize<HouseKeyPayload>(jsonContent);
            if (payload == null || string.IsNullOrWhiteSpace(payload.Signature))
                return false;

            var expectedSignature = ComputeSignature(payload.FamilyId, payload.Version, payload.IssuedAtUtc);

            // Fixed-time comparison protects against side-channel timing attacks
            var isSignatureValid = CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(payload.Signature),
                Encoding.UTF8.GetBytes(expectedSignature)
            );

            if (isSignatureValid)
            {
                familyId = payload.FamilyId;
                return true;
            }

            return false;
        }
        catch
        {
            // Invalid JSON or deserialization structure mismatch
            return false;
        }
    }

    private string ComputeSignature(Guid familyId, int version, DateTime issuedAtUtc)
    {
        var rawData = $"{familyId}:{version}:{issuedAtUtc:O}";
        using var hmac = new HMACSHA256(_signingKey);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        return Convert.ToBase64String(hash);
    }
}